using FirePDF.Model;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FirePDF.Writing
{
    public class PDFWriter : IDisposable
    {
        private readonly Stream stream;
        private readonly byte[] buffer;
        private int bufferIndex;
        private readonly bool leaveOpen;

        private readonly XREFTable newTable;

        private PDF pdf;
        private Dictionary<ObjectReference, ObjectReference> indirectReferenceRemap;

        public PDFWriter(Stream stream, bool leaveOpen)
        {
            this.stream = stream;
            this.leaveOpen = leaveOpen;

            this.buffer = new byte[1024];
            this.bufferIndex = 0;

            indirectReferenceRemap = new Dictionary<ObjectReference, ObjectReference>();
            newTable = new XREFTable();
        }

        public void flush()
        {
            if (bufferIndex > 0)
            {
                stream.Write(buffer, 0, bufferIndex);
                bufferIndex = 0;
            }
        }

        public void writeHeader(double version)
        {
            writeHeader((float)version);
        }

        public void writeHeader(float version)
        {
            writeASCII("%PDF-");
            writeASCII(version);
            writeByte(0x0a);
            writeBytes(new byte[] { 0x25, 0xe2, 0xe3, 0xcf, 0xd3 });
            writeNewLine();
        }

        public void writeNewPDF(PDF pdf)
        {
            this.pdf = pdf;
            writeHeader(pdf.version);

            ObjectReference trailerRef = pdf.store.add(pdf.underlyingDict);
            
            HashSet<ObjectReference> allObjects = exploreRecursively(trailerRef);
            
            Dictionary<ObjectReference, ObjectReference> map = remapDirtyObjects(allObjects);
            
            if (map.ContainsKey(trailerRef))
            {
                trailerRef = map[trailerRef];
            }

            allObjects.Remove(trailerRef);

            allObjects = remapReferences(allObjects, map);
            writeIndirectObjects(allObjects);

            flush();
            stream.Flush();
            
            long xrefOffset = stream.Position;
            newTable.serialize(this);

            Trailer trailer = new Trailer(pdf.get<PDFDictionary>(trailerRef));
            trailer.serialize(this);

            writeASCII("startxref");
            writeNewLine();

            writeASCII(xrefOffset);
            writeNewLine();

            writeASCII("%%EOF");

            flush();
            stream.Flush();
        }

        private HashSet<ObjectReference> exploreRecursively(ObjectReference root)
        {
            HashSet<ObjectReference> discoveredObjects = new HashSet<ObjectReference>();

            Queue<ObjectReference> queue = new Queue<ObjectReference>();
            queue.Enqueue(root);

            while (queue.Count != 0)
            {
                ObjectReference objRef = queue.Dequeue();
                discoveredObjects.Add(objRef);

                object obj = objRef.get<object>();
                if (obj is IHaveChildren)
                {
                    foreach (ObjectReference subRef in (obj as IHaveChildren).GetObjectReferences())
                    {
                        if (discoveredObjects.Contains(subRef) == false)
                        {
                            discoveredObjects.Add(subRef);
                            queue.Enqueue(subRef);
                        }
                    }
                }
            }

            return discoveredObjects;
        }

        private Dictionary<ObjectReference, ObjectReference> remapDirtyObjects(HashSet<ObjectReference> objectReferences)
        {
            Dictionary<ObjectReference, ObjectReference> map = new Dictionary<ObjectReference, ObjectReference>();
            
            bool hasChanged = true;
            while (hasChanged)
            {
                hasChanged = false;
                foreach (ObjectReference temp in objectReferences)
                {
                    ObjectReference reference = map.ContainsKey(temp) ? map[temp] : temp;

                    object obj = reference.get<object>();
                    if (obj is IHaveChildren == false)
                    {
                        continue;
                    }

                    (obj as IHaveChildren).swapReferences(x => map.ContainsKey(x) ? map[x] : x);
                }

                foreach (ObjectReference temp in objectReferences)
                {
                    ObjectReference reference = map.ContainsKey(temp) ? map[temp] : temp;

                    object obj = reference.get<object>();
                    if (obj is IHaveChildren == false)
                    {
                        continue;
                    }

                    if (pdf.store.isExistingObject(reference))
                    {
                        if ((obj as IHaveChildren).isDirty())
                        {
                            ObjectReference newObjRef = pdf.store.add(obj);
                            map[reference] = newObjRef;
                            hasChanged = true;
                        }
                    }
                }
            }

            return map;
        }

        private HashSet<ObjectReference> remapReferences(HashSet<ObjectReference> references, Dictionary<ObjectReference, ObjectReference> map)
        {
            HashSet<ObjectReference> newReferences = new HashSet<ObjectReference>();
            foreach(ObjectReference reference in references)
            {
                if(map.ContainsKey(reference))
                {
                    newReferences.Add(map[reference]);
                }
                else
                {
                    newReferences.Add(reference);
                }
            }

            return newReferences;
        }

        private void writeIndirectObjects(IEnumerable<ObjectReference> objectReferences)
        {
            foreach(ObjectReference objectRef in objectReferences.OrderBy(x => x.objectNumber))
            {
                object value = objectRef.get<object>();
                writeIndirectObject(objectRef.objectNumber, objectRef.generation, value);
            }
        }
        
        public void writeUpdatedPDF(PDF pdf)
        {
            this.pdf = pdf;
           
            ObjectReference trailerRef = pdf.store.add(pdf.underlyingDict);

            HashSet<ObjectReference> allObjects = exploreRecursively(trailerRef);
            Dictionary<ObjectReference, ObjectReference> map = remapDirtyObjects(allObjects);

            if (map.ContainsKey(trailerRef))
            {
                trailerRef = map[trailerRef];
            }

            allObjects.Remove(trailerRef);

            allObjects = remapReferences(allObjects, map);

            int existingObjects = allObjects.RemoveWhere(x => pdf.store.isExistingObject(x));

            if(allObjects.Count == 0)
            {
                //no new objects to write
                stream.Flush();
                return;
            }

            writeIndirectObjects(allObjects);

            long xrefOffset = stream.Position + bufferIndex;
            newTable.serialize(this);

            Trailer trailer = new Trailer(pdf.get<PDFDictionary>(trailerRef));
            trailer.prev = (int)pdf.lastXrefOffset;
            trailer.serialize(this);

            writeASCII("startxref");
            writeNewLine();

            writeASCII(xrefOffset);
            writeNewLine();

            writeASCII("%%EOF");

            flush();
            stream.Flush();
        }
        
        public void writeIndirectObject(int objectNumber, int generation, object obj)
        {
            newTable.addRecord(new XREFTable.XREFRecord
            {
                isCompressed = false,
                objectNumber = objectNumber,
                generation = generation,
                offset = stream.Position + bufferIndex
            });

            writeASCII(objectNumber + " " + generation + " obj");
            writeNewLine();
            writeDirectObject(obj);
            writeNewLine();
            writeASCII("endobj");
            writeNewLine();
        }

        public void writeDirectObject(object obj)
        {
            if (obj is PDFDictionary)
            {
                writeDirectObject(obj as PDFDictionary);
            }
            else if (obj is PDFStream)
            {
                PDFStream streamObj = obj as PDFStream;
                writeDirectObject(streamObj.underlyingDict);
                writeNewLine();
                writeASCII("stream");
                writeNewLine();
                writeStream(streamObj.getCompressedStream());

                writeASCII("endstream");
            }
            else if (obj is IHaveUnderlyingDict)
            {
                writeDirectObject((obj as IHaveUnderlyingDict).underlyingDict);
            }
            else if (obj is PDFList)
            {
                writeDirectObject(obj as PDFList);
            }
            else if (obj is Name)
            {
                writeDirectObject(obj as Name);
            }
            else if (obj is ObjectReference)
            {
                writeDirectObject(obj as ObjectReference);
            }
            else if (obj is XObjectForm)
            {
                throw new Exception("should never happen! call XObjectForm.serialize() instead.");
            }
            else if (obj is byte[])
            {
                writeDirectObject(obj as byte[]);
            }
            else if (obj is bool)
            {
                writeASCII((bool)obj);
            }
            else if (obj is float)
            {
                writeASCII((float)obj);
            }
            else if (obj is int)
            {
                writeASCII((int)obj);
            }
            else if (obj is double)
            {
                writeASCII((double)obj);
            }
            else if (obj is long)
            {
                writeASCII((long)obj);
            }
            else if (obj is string)
            {
                writeDirectObject(obj as string);
            }
            else if (obj is null)
            {
                writeASCII("null");
            }
            else
            {
                throw new NotImplementedException();
            }
        }

        public void writeDirectObject(byte[] hexString)
        {
            StringBuilder hex = new StringBuilder(hexString.Length * 2);

            foreach (byte b in hexString)
                hex.AppendFormat("{0:x2}", b);

            writeASCII(hex.ToString());
        }

        public void writeDirectObject(string s)
        {
            StringBuilder sb = new StringBuilder();
            sb.Append("(");

            //TODO for named codes (e.g. \014 == \f) we should write the named code and save space

            foreach (char c in s)
            {
                if (c < 32 || c > 127)
                {
                    string s2 = Convert.ToString(c, 8);
                    sb.Append("\\" + Convert.ToString(c, 8).PadLeft(3, '0'));
                }
                else if(c == '\\' || c == '(' || c == ')')
                {
                    sb.Append("\\" + c);
                }
                else
                {
                    sb.Append(c);
                }
            }

            sb.Append(")");

            writeASCII(sb.ToString());
        }

        public void writeDirectObject(ObjectReference objectReference)
        {
            if (indirectReferenceRemap.ContainsKey(objectReference))
            {
                throw new Exception("should never happen");
            }

            writeASCII(objectReference.objectNumber + " " + objectReference.generation + " R");
        }

        public void writeDirectObject(Name name)
        {
            writeASCII("/" + name);
        }

        public void writeDirectObject(PDFList array)
        {
            writeASCII("[");
            bool isFirst = true;

            foreach (object element in array)
            {
                if (isFirst)
                {
                    isFirst = false;
                }
                else
                {
                    writeASCII(" ");
                }

                writeDirectObject(element);
            }
            writeASCII("]");
        }

        public void writeDirectObject(PDFDictionary dict)
        {
            writeASCII("<<");
            foreach (var pair in dict)
            {
                if (pair.Value is Name || pair.Value is PDFList || pair.Value is PDFDictionary)
                {
                    //we don't need a space for certain value types
                    writeASCII("/" + pair.Key);
                }
                else
                {
                    writeASCII("/" + pair.Key + " ");
                }

                writeDirectObject(pair.Value);
            }
            writeASCII(">>");
        }

        public void writeNewLine()
        {
            if (buffer.Length - bufferIndex < 3)
            {
                flush();
            }

            buffer[bufferIndex++] = 0x0d;
            buffer[bufferIndex++] = 0x0a;
        }

        public void writeASCII(bool b)
        {
            writeASCII(b ? "true" : "false");
        }

        public void writeASCII(float f)
        {
            writeASCII(f.ToString());
        }

        public void writeASCII(double d)
        {
            writeASCII(d.ToString());
        }

        public void writeASCII(long l)
        {
            writeASCII(l.ToString());
        }

        public void writeASCII(int i)
        {
            writeASCII(i.ToString());
        }

        public void writeASCII(string s)
        {
            writeBytes(Encoding.ASCII.GetBytes(s));
        }

        private void writeByte(byte b)
        {
            if (buffer.Length - bufferIndex < 2)
            {
                flush();
            }

            buffer[bufferIndex++] = b;
        }

        public void writeBytes(byte[] bytes)
        {
            if (buffer.Length - bufferIndex > bytes.Length)
            {
                Array.Copy(bytes, 0, buffer, bufferIndex, bytes.Length);
                bufferIndex += bytes.Length;
            }
            else if (buffer.Length > bytes.Length)
            {
                flush();
                Array.Copy(bytes, 0, buffer, bufferIndex, bytes.Length);
                bufferIndex = bytes.Length;
            }
            else
            {
                flush();
                stream.Write(bytes, 0, bytes.Length);
            }
        }

        public void writeStream(Stream stream)
        {
            flush();

            stream.CopyTo(this.stream);
        }

        public void Dispose()
        {
            flush();
            if (leaveOpen == false)
            {
                stream.Dispose();
            }
        }
    }
}
