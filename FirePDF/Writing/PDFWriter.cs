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

        public PDFWriter(Stream stream, bool leaveOpen)
        {
            this.stream = stream;
            this.leaveOpen = leaveOpen;

            this.buffer = new byte[1024];
            this.bufferIndex = 0;
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
            foreach (Page page in pdf.catalog)
            {
                if (page.isDirty)
                {
                    writePage(page);
                }
            }
        }

        public void writePage(Page page)
        {
            if (page.resources.isDirty)
            {
                writeResources(page.resources);
            }
        }

        public void writeResources(PDFResources resources)
        {
            foreach(string dirtyObjectPath in resources.dirtyObjects)
            {
                string[] path = dirtyObjectPath.Split('/');
                object obj = resources.getObjectAtPath(path);
                writeIndirectObject(0, 0, obj);
            }

            writeIndirectObject(0, 0, resources.underlyingDict);
        }

        public void writeIndirectObject(int objectNumber, int generation, object obj)
        {
            writeASCII(objectNumber + " " + generation + " obj");
            writeDirectObject(obj);
            writeASCII("endobj");
        }

        public void writeDirectObject(object obj)
        {
            if(obj is Dictionary<Name, object>)
            {
                writeDirectObject(obj as Dictionary<Name, object>);
            }
            else if(obj is List<object>)
            {
                writeDirectObject(obj as List<object>);
            }
            else
            {
                throw new NotImplementedException();
            }
        }

        public void writeDirectObject(List<object> array)
        {
            writeASCII("[");
            bool isFirst = true;

            foreach(object element in array)
            {
                if(isFirst)
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
            foreach(var pair in dict)
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
