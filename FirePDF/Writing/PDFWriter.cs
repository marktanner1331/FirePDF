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

        private XREFTable readTable;
        private XREFTable writeTable;
        private int nextObjectNumber;

        public PDFWriter(Stream stream, bool leaveOpen)
        {
            this.stream = stream;
            this.leaveOpen = leaveOpen;

            this.buffer = new byte[1024];
            this.bufferIndex = 0;

            this.writeTable = new XREFTable();
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

        public void updatePDF(PDF pdf)
        {
            this.readTable = pdf.readableTable;

            //not modifying the existing readTable here makes more sense to me
            //this is becuase the existing records have very valid pointers to existing objects
            //don't want to screw with that unnecessarily
            writeTable.clear();

            ObjectReference root;
            if (pdf.catalog.isDirty)
            {
                root = pdf.catalog.serialize(this);
            }
            else
            {
                throw new NotImplementedException();
            }

            long xrefOffset = stream.Position + bufferIndex;
            writeTable.serialize(this);

            Trailer trailer = new Trailer();
            trailer.root = root;

            if (pdf.offsetOfLastXRefTable != null)
            {
                trailer.prev = (int)pdf.offsetOfLastXRefTable;
            }
            trailer.serialize(this);

            writeASCII("startxref");
            writeNewLine();

            writeASCII(xrefOffset);
            writeNewLine();

            writeASCII("%%EOF");
        }

        public ObjectReference writeIndirectObjectUsingNextFreeNumber(object obj)
        {
            if (obj is XObjectForm)
            {
                return ((XObjectForm)obj).serialize(this);
            }

            int nextFreeNumber2 = readTable.getNextFreeRecordNumber();
            XREFTable.XREFRecord pageRecord = new XREFTable.XREFRecord
            {
                isCompressed = false,
                objectNumber = nextFreeNumber2,
                generation = 0,
                offset = stream.Position + bufferIndex
            };
            readTable.addRecord(pageRecord);
            writeTable.addRecord(pageRecord);

            writeIndirectObject(pageRecord.objectNumber, pageRecord.generation, obj);
            return new ObjectReference(pageRecord.objectNumber, pageRecord.generation);
        }

        public ObjectReference writeIndirectObjectUsingNextFreeNumber(PDFDictionary streamDictionary, Stream stream)
        {
            int nextFreeNumber2 = readTable.getNextFreeRecordNumber();
            XREFTable.XREFRecord pageRecord = new XREFTable.XREFRecord
            {
                isCompressed = false,
                objectNumber = nextFreeNumber2,
                generation = 0,
                offset = this.stream.Position + bufferIndex
            };
            readTable.addRecord(pageRecord);
            writeTable.addRecord(pageRecord);

            writeIndirectObject(pageRecord.objectNumber, pageRecord.generation, streamDictionary, stream);
            return new ObjectReference(pageRecord.objectNumber, pageRecord.generation);
        }

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

        public void writeIndirectObject(int objectNumber, int generation, PDFDictionary streamDictionary, Stream stream)
        {
            writeASCII(objectNumber + " " + generation + " obj");
            writeNewLine();
            writeDirectObject(streamDictionary);
            writeNewLine();
            writeASCII("stream");
            writeNewLine();

            flush();

            int length = (int)streamDictionary["Length"];
            while (length > 0)
            {
                byte[] buffer = new byte[1024];
                int bytesRead = stream.Read(buffer, 0, Math.Min(length, buffer.Length));

                this.stream.Write(buffer, 0, bytesRead);
                length -= bytesRead;
            }

            writeNewLine();

            writeASCII("endstream");
            writeNewLine();

            writeASCII("endobj");
            writeNewLine();
        }

        public void writeIndirectObject(int objectNumber, int generation, object obj)
        {
            writeASCII(objectNumber + " " + generation + " obj");
            writeNewLine();
            writeDirectObject(obj);
            writeNewLine();
            writeASCII("endobj");
            writeNewLine();
        }

        public void writeDirectObject(object obj)
        {
            if (obj is Dictionary<Name, object>)
            {
                writeDirectObject(obj as Dictionary<Name, object>);
            }
            else if (obj is List<object>)
            {
                writeDirectObject(obj as List<object>);
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
            writeASCII(objectReference.objectNumber + " " + objectReference.generation + " R");
        }

        public void writeDirectObject(Name name)
        {
            writeASCII("/" + name);
        }

        public void writeDirectObject(List<object> array)
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

        public void writeDirectObject(Dictionary<Name, object> dict)
        {
            writeASCII("<<");
            foreach (var pair in dict)
            {
                writeASCII("/" + pair.Key + " ");
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
