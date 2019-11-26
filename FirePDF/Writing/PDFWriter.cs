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

            indirectReferenceRemap.Clear();
            remapDirtyExistingObjects(pdf.catalog.underlyingDict);

            ObjectReference catalogRef = pdf.store.add(pdf.catalog.underlyingDict);
            writeRecursively(catalogRef);
            
            long xrefOffset = stream.Position + bufferIndex;
            newTable.serialize(this);

            Trailer trailer = new Trailer(pdf);
            trailer.root = catalogRef;

            trailer.serialize(this);

            writeASCII("startxref");
            writeNewLine();

            writeASCII(xrefOffset);
            writeNewLine();

            writeASCII("%%EOF");

            flush();
            stream.Flush();
        }

        private void writeRecursively(ObjectReference root)
        {
            HashSet<ObjectReference> completed = new HashSet<ObjectReference>();

            Queue<ObjectReference> queue = new Queue<ObjectReference>();
            queue.Enqueue(root);

            while(queue.Count > 0)
            {
                ObjectReference objectRef = queue.Dequeue();
                if (indirectReferenceRemap.ContainsKey(objectRef))
                {
                    objectRef = indirectReferenceRemap[objectRef];
                }

                if (completed.Contains(objectRef))
                {
                    continue;
                }

                completed.Add(objectRef);
                
                object value = objectRef.get<object>();
                
                writeIndirectObject(objectRef.objectNumber, objectRef.generation, value);

                if (value is IHaveChildren && !(value is ObjectReference))
                {
                    List<ObjectReference> newReferences = (value as IHaveChildren).GetObjectReferences().ToList();
                    foreach(ObjectReference newReference in newReferences)
                    {
                        queue.Enqueue(newReference);
                    }
                }
            }
        }

        private void remapDirtyExistingObjects(object obj)
        {
            if(obj is PDFDictionary)
            {
                PDFDictionary dict = obj as PDFDictionary;
                foreach(Name key in dict.keys)
                {
                    object value = dict.get<object>(key, false);
                    if (value is ObjectReference)
                    {
                        ObjectReference objRef = value as ObjectReference;
                        if (indirectReferenceRemap.ContainsKey(objRef))
                        {
                            objRef = indirectReferenceRemap[objRef];
                            dict.set(key, objRef);
                        }
                        else if (pdf.store.isExistingObject(objRef) && objRef.isDirty)
                        {
                            ObjectReference newObjRef = pdf.store.add(objRef.get<object>(), false);
                            indirectReferenceRemap[objRef] = newObjRef;
                            dict.set(key, newObjRef);
                        }
                    }
                    else
                    {
                        remapDirtyExistingObjects(value);
                    }
                }
            }
            else if(obj is IHaveUnderlyingDict)
            {
                remapDirtyExistingObjects((obj as IHaveUnderlyingDict).underlyingDict);
            }
            else if(obj is PDFList)
            {
                int i = 0;
                foreach (var subObj in (obj as PDFList).ToList())
                {
                    if (subObj is ObjectReference)
                    {
                        ObjectReference objRef = subObj as ObjectReference;
                        if (indirectReferenceRemap.ContainsKey(objRef))
                        {
                            objRef = indirectReferenceRemap[objRef];
                            (obj as PDFList).set(i, objRef);
                        }
                        else if (pdf.store.isExistingObject(objRef) && objRef.isDirty)
                        {
                            ObjectReference newObjRef = pdf.store.add(objRef.get<object>(), false);
                            indirectReferenceRemap[objRef] = newObjRef;
                            (obj as PDFList).set(i, newObjRef);
                        }
                    }
                    else
                    {
                        remapDirtyExistingObjects(subObj);
                    }

                    i++;
                }
            }
        }
        
        public void writeUpdatedPDF(PDF pdf)
        {
            //this.readTable = pdf.readableTable;

            ////not modifying the existing readTable here makes more sense to me
            ////this is becuase the existing records have very valid pointers to existing objects
            ////don't want to screw with that unnecessarily
            //writeTable.clear();

            //ObjectReference root;
            //if (pdf.catalog.isDirty)
            //{
            //    root = pdf.catalog.serialize(this);
            //}
            //else
            //{
            //    throw new NotImplementedException();
            //}

            //long xrefOffset = stream.Position + bufferIndex;
            //writeTable.serialize(this);

            //Trailer trailer = new Trailer();
            //trailer.root = root;

            //if (pdf.offsetOfLastXRefTable != null)
            //{
            //    trailer.prev = (int)pdf.offsetOfLastXRefTable;
            //}
            //trailer.serialize(this);

            //writeASCII("startxref");
            //writeNewLine();

            //writeASCII(xrefOffset);
            //writeNewLine();

            //writeASCII("%%EOF");
        }

        //public ObjectReference writeIndirectObjectUsingNextFreeNumber(PDF pdf, object obj)
        //{
        //    if (obj is XObjectForm)
        //    {
        //        return ((XObjectForm)obj).serialize(this);
        //    }

        //    int nextFreeNumber2 = readTable.getNextFreeRecordNumber();
        //    XREFTable.XREFRecord pageRecord = new XREFTable.XREFRecord
        //    {
        //        isCompressed = false,
        //        objectNumber = nextFreeNumber2,
        //        generation = 0,
        //        offset = stream.Position + bufferIndex
        //    };
        //    readTable.addRecord(pageRecord);
        //    writeTable.addRecord(pageRecord);

        //    writeIndirectObject(pageRecord.objectNumber, pageRecord.generation, obj);
        //    return new ObjectReference(pdf, pageRecord.objectNumber, pageRecord.generation);
        //}

        //public ObjectReference writeIndirectObjectUsingNextFreeNumber(PDF pdf, PDFDictionary streamDictionary, Stream stream)
        //{
        //    int nextFreeNumber2 = readTable.getNextFreeRecordNumber();
        //    XREFTable.XREFRecord pageRecord = new XREFTable.XREFRecord
        //    {
        //        isCompressed = false,
        //        objectNumber = nextFreeNumber2,
        //        generation = 0,
        //        offset = this.stream.Position + bufferIndex
        //    };
        //    readTable.addRecord(pageRecord);
        //    writeTable.addRecord(pageRecord);

        //    writeIndirectObject(pageRecord.objectNumber, pageRecord.generation, streamDictionary, stream);
        //    return new ObjectReference(pdf, pageRecord.objectNumber, pageRecord.generation);
        //}

        //public void writeDirtyResources(PDFResources resources)
        //{
        //    foreach (string dirtyObjectPath in resources.dirtyObjects)
        //    {
        //        string[] path = dirtyObjectPath.Split('/');
        //        object obj = resources.getObjectAtPath(path);

        //        ObjectReference objectRef = writeIndirectObjectUsingNextFreeNumber(obj);
        //        resources.setObjectAtPath(objectRef, path);
        //    }

        //    resources.dirtyObjects.Clear();
        //}

        //public void writeIndirectObject(int objectNumber, int generation, PDFDictionary streamDictionary, Stream stream)
        //{
        //    newTable.addRecord(new XREFTable.XREFRecord
        //    {
        //        isCompressed = false,
        //        objectNumber = objectNumber,
        //        generation = generation,
        //        offset = stream.Position + bufferIndex
        //    });

        //    writeASCII(objectNumber + " " + generation + " obj");
        //    writeNewLine();
        //    writeDirectObject(streamDictionary);
        //    writeNewLine();
        //    writeASCII("stream");
        //    writeNewLine();

        //    flush();

        //    int length = streamDictionary.get<int>("Length");
        //    while (length > 0)
        //    {
        //        byte[] buffer = new byte[1024];
        //        int bytesRead = stream.Read(buffer, 0, Math.Min(length, buffer.Length));

        //        this.stream.Write(buffer, 0, bytesRead);
        //        length -= bytesRead;
        //    }

        //    writeNewLine();

        //    writeASCII("endstream");
        //    writeNewLine();

        //    writeASCII("endobj");
        //    writeNewLine();
        //}

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
            else if(obj is PDFStream)
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
            else if(obj is bool)
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

            foreach (char c in s)
            {
                if (c < 32 || c > 127)
                {
                    string s2 = Convert.ToString(c, 8);
                    sb.Append("\\" + Convert.ToString(c, 8).PadLeft(3, '0'));
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
            if(indirectReferenceRemap.ContainsKey(objectReference))
            {
                throw new Exception("should never happen");
                objectReference = indirectReferenceRemap[objectReference];
            }
            else
            {
                //if (pdf.store.isExistingObject(objectReference) && pdf.store.isDirty(objectReference))
                //{
                //    throw new Exception("should never happen");
                //}
                    //ObjectReference newReference = pdf.store.add(objectReference.get<object>(), false);
                    //indirectReferenceRemap[objectReference] = newReference;
                    //objectReference = newReference;
                //}
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
                if(pair.Value is Name || pair.Value is PDFList || pair.Value is PDFDictionary)
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
