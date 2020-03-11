using FirePDF.Model;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;

namespace FirePDF.Writing
{
    public class PdfWriter : IDisposable
    {
        private readonly Stream stream;
        private readonly byte[] buffer;
        private int bufferIndex;
        private readonly bool leaveOpen;

        private readonly XrefTable newTable;

        private Pdf pdf;
        private readonly Dictionary<ObjectReference, ObjectReference> indirectReferenceRemap;

        public PdfWriter(Stream stream, bool leaveOpen)
        {
            this.stream = stream;
            this.leaveOpen = leaveOpen;

            buffer = new byte[1024];
            bufferIndex = 0;

            indirectReferenceRemap = new Dictionary<ObjectReference, ObjectReference>();
            newTable = new XrefTable();
        }

        public void Flush()
        {
            if (bufferIndex > 0)
            {
                stream.Write(buffer, 0, bufferIndex);
                bufferIndex = 0;
            }
        }

        public void WriteHeader(double version)
        {
            WriteHeader((float)version);
        }

        public void WriteHeader(float version)
        {
            WriteAscii("%PDF-");
            WriteAscii(version);
            WriteByte(0x0a);
            WriteBytes(new byte[] { 0x25, 0xe2, 0xe3, 0xcf, 0xd3 });
            WriteNewLine();
        }

        public void WriteNewPdf(Pdf pdf)
        {
            this.pdf = pdf;
            WriteHeader(pdf.version);

            ObjectReference trailerRef = pdf.store.Add(pdf.UnderlyingDict);
            
            HashSet<ObjectReference> allObjects = ExploreRecursively(trailerRef);
            
            Dictionary<ObjectReference, ObjectReference> map = RemapDirtyObjects(allObjects);
            
            if (map.ContainsKey(trailerRef))
            {
                trailerRef = map[trailerRef];
            }

            allObjects.Remove(trailerRef);

            allObjects = RemapReferences(allObjects, map);
            WriteIndirectObjects(allObjects);

            Flush();
            stream.Flush();
            
            long xrefOffset = stream.Position;
            newTable.Serialize(this);

            Trailer trailer = new Trailer(pdf.Get<PdfDictionary>(trailerRef));
            trailer.Serialize(this);

            WriteAscii("startxref");
            WriteNewLine();

            WriteAscii(xrefOffset);
            WriteNewLine();

            WriteAscii("%%EOF");

            Flush();
            stream.Flush();
        }

        private static HashSet<ObjectReference> ExploreRecursively(ObjectReference root)
        {
            HashSet<ObjectReference> discoveredObjects = new HashSet<ObjectReference>();

            Queue<ObjectReference> queue = new Queue<ObjectReference>();
            queue.Enqueue(root);

            while (queue.Count != 0)
            {
                ObjectReference objRef = queue.Dequeue();
                discoveredObjects.Add(objRef);

                object obj = objRef.Get<object>();
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

        private Dictionary<ObjectReference, ObjectReference> RemapDirtyObjects(HashSet<ObjectReference> objectReferences)
        {
            Dictionary<ObjectReference, ObjectReference> map = new Dictionary<ObjectReference, ObjectReference>();
            
            bool hasChanged = true;
            while (hasChanged)
            {
                hasChanged = false;
                foreach (ObjectReference temp in objectReferences)
                {
                    ObjectReference reference = map.ContainsKey(temp) ? map[temp] : temp;

                    object obj = reference.Get<object>();
                    if (obj is IHaveChildren == false)
                    {
                        continue;
                    }

                    (obj as IHaveChildren).SwapReferences(x => map.ContainsKey(x) ? map[x] : x);
                }

                foreach (ObjectReference temp in objectReferences)
                {
                    ObjectReference reference = map.ContainsKey(temp) ? map[temp] : temp;

                    object obj = reference.Get<object>();
                    if (obj is IHaveChildren == false)
                    {
                        continue;
                    }

                    if (pdf.store.IsExistingObject(reference))
                    {
                        if ((obj as IHaveChildren).IsDirty())
                        {
                            ObjectReference newObjRef = pdf.store.Add(obj);
                            map[reference] = newObjRef;
                            hasChanged = true;
                        }
                    }
                }
            }

            return map;
        }

        private static HashSet<ObjectReference> RemapReferences(IEnumerable<ObjectReference> references, Dictionary<ObjectReference, ObjectReference> map)
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

        private void WriteIndirectObjects(IEnumerable<ObjectReference> objectReferences)
        {
            foreach(ObjectReference objectRef in objectReferences.OrderBy(x => x.objectNumber))
            {
                object value = objectRef.Get<object>();
                if (objectRef.objectNumber == 60)
                {

                }
                WriteIndirectObject(objectRef.objectNumber, objectRef.generation, value);
            }
        }
        
        public void WriteUpdatedPdf(Pdf pdf)
        {
            this.pdf = pdf;
           
            ObjectReference trailerRef = pdf.store.Add(pdf.UnderlyingDict);

            HashSet<ObjectReference> allObjects = ExploreRecursively(trailerRef);
            Dictionary<ObjectReference, ObjectReference> map = RemapDirtyObjects(allObjects);

            if (map.ContainsKey(trailerRef))
            {
                trailerRef = map[trailerRef];
            }

            allObjects.Remove(trailerRef);

            allObjects = RemapReferences(allObjects, map);

            int existingObjects = allObjects.RemoveWhere(x => pdf.store.IsExistingObject(x));

            if(allObjects.Count == 0)
            {
                //no new objects to write
                stream.Flush();
                return;
            }

            WriteIndirectObjects(allObjects);

            long xrefOffset = stream.Position + bufferIndex;
            newTable.Serialize(this);

            Trailer trailer = new Trailer(pdf.Get<PdfDictionary>(trailerRef));
            trailer.Prev = (int)pdf.lastXrefOffset;
            trailer.Serialize(this);

            WriteAscii("startxref");
            WriteNewLine();

            WriteAscii(xrefOffset);
            WriteNewLine();

            WriteAscii("%%EOF");

            Flush();
            stream.Flush();
        }
        
        public void WriteIndirectObject(int objectNumber, int generation, object obj)
        {
            newTable.AddRecord(new XrefTable.XrefRecord
            {
                isCompressed = false,
                objectNumber = objectNumber,
                generation = generation,
                offset = stream.Position + bufferIndex
            });

            WriteAscii(objectNumber + " " + generation + " obj");
            WriteNewLine();
            WriteDirectObject(obj);
            WriteNewLine();
            WriteAscii("endobj");
            WriteNewLine();
        }

        public void WriteDirectObject(object obj)
        {
            if (obj is PdfDictionary)
            {
                WriteDirectObject(obj as PdfDictionary);
            }
            else if (obj is PdfStream)
            {
                PdfStream streamObj = obj as PdfStream;
                WriteDirectObject(streamObj.UnderlyingDict);
                WriteNewLine();
                WriteAscii("stream");
                WriteNewLine();
                WriteStream(streamObj.GetCompressedStream());
                WriteNewLine();
                WriteAscii("endstream");
            }
            else if(obj is Font font)
            {
                font.PrepareForWriting();
                WriteDirectObject((obj as HaveUnderlyingDict).UnderlyingDict);
            }
            else if (obj is HaveUnderlyingDict)
            {
                WriteDirectObject((obj as HaveUnderlyingDict).UnderlyingDict);
            }
            else if (obj is PdfList)
            {
                WriteDirectObject(obj as PdfList);
            }
            else if (obj is Name)
            {
                WriteDirectObject(obj as Name);
            }
            else if (obj is ObjectReference)
            {
                WriteDirectObject(obj as ObjectReference);
            }
            else if (obj is XObjectForm)
            {
                throw new Exception("should never happen! call XObjectForm.serialize() instead.");
            }
            else if (obj is byte[])
            {
                WriteDirectObject(obj as byte[]);
            }
            else if (obj is bool)
            {
                WriteAscii((bool)obj);
            }
            else if (obj is float)
            {
                WriteAscii((float)obj);
            }
            else if (obj is int)
            {
                WriteAscii((int)obj);
            }
            else if (obj is double)
            {
                WriteAscii((double)obj);
            }
            else if (obj is long)
            {
                WriteAscii((long)obj);
            }
            else if (obj is PdfString pdfString)
            {
                if(pdfString.isHexString)
                {
                    WriteDirectObject(pdfString.ToByteArray());
                }
                else
                {
                    WriteDirectObject(obj.ToString());
                }
                
            }
            else if (obj is null)
            {
                WriteAscii("null");
            }
            else
            {
                throw new NotImplementedException();
            }
        }

        public void WriteDirectObject(byte[] hexString)
        {
            StringBuilder hex = new StringBuilder(hexString.Length * 2);

            foreach (byte b in hexString)
                hex.AppendFormat("{0:x2}", b);

            WriteAscii("<" + hex.ToString() + ">");
        }

        public void WriteDirectObject(string s)
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

            WriteAscii(sb.ToString());
        }

        public void WriteDirectObject(ObjectReference objectReference)
        {
            if (indirectReferenceRemap.ContainsKey(objectReference))
            {
                throw new Exception("should never happen");
            }

            WriteAscii(objectReference.objectNumber + " " + objectReference.generation + " R");
        }

        public void WriteDirectObject(Name name)
        {
            WriteAscii("/" + name);
        }

        public void WriteDirectObject(PdfList array)
        {
            WriteAscii("[");
            bool isFirst = true;

            foreach (object element in array)
            {
                if (isFirst)
                {
                    isFirst = false;
                }
                else
                {
                    WriteAscii(" ");
                }

                WriteDirectObject(element);
            }
            WriteAscii("]");
        }

        public void WriteDirectObject(PdfDictionary dict)
        {
            WriteAscii("<<");
            foreach (KeyValuePair<Name, object> pair in dict)
            {
                if (pair.Value is Name || pair.Value is PdfList || pair.Value is PdfDictionary)
                {
                    //we don't need a space for certain value types
                    WriteAscii("/" + pair.Key);
                }
                else
                {
                    WriteAscii("/" + pair.Key + " ");
                }

                WriteDirectObject(pair.Value);
            }
            WriteAscii(">>");
        }

        public void WriteNewLine()
        {
            if (buffer.Length - bufferIndex < 3)
            {
                Flush();
            }

            buffer[bufferIndex++] = 0x0d;
            buffer[bufferIndex++] = 0x0a;
        }

        public void WriteAscii(bool b)
        {
            WriteAscii(b ? "true" : "false");
        }

        public void WriteAscii(float f)
        {
            WriteAscii(Math.Round(f, 2).ToString(CultureInfo.InvariantCulture));
        }

        public void WriteAscii(double d)
        {
            WriteAscii(Math.Round(d, 2).ToString(CultureInfo.InvariantCulture));
        }

        public void WriteAscii(long l)
        {
            WriteAscii(l.ToString());
        }

        public void WriteAscii(int i)
        {
            WriteAscii(i.ToString());
        }

        public void WriteAscii(string s)
        {
            WriteBytes(Encoding.ASCII.GetBytes(s));
        }

        private void WriteByte(byte b)
        {
            if (buffer.Length - bufferIndex < 2)
            {
                Flush();
            }

            buffer[bufferIndex++] = b;
        }

        public void WriteBytes(byte[] bytes)
        {
            if (buffer.Length - bufferIndex > bytes.Length)
            {
                Array.Copy(bytes, 0, buffer, bufferIndex, bytes.Length);
                bufferIndex += bytes.Length;
            }
            else if (buffer.Length > bytes.Length)
            {
                Flush();
                Array.Copy(bytes, 0, buffer, bufferIndex, bytes.Length);
                bufferIndex = bytes.Length;
            }
            else
            {
                Flush();
                stream.Write(bytes, 0, bytes.Length);
            }
        }

        public void WriteStream(Stream stream)
        {
            Flush();

            stream.CopyTo(this.stream);
        }

        public void Dispose()
        {
            Flush();
            if (leaveOpen == false)
            {
                stream.Dispose();
            }
        }
    }
}
