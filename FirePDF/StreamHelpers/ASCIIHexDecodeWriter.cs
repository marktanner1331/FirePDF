using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FirePDF.StreamHelpers
{
    public static class ASCIIHexDecodeWriter
    {
        public static void encode(Stream source, Stream destination)
        {
            using (StreamWriter writer = new StreamWriter(destination, Encoding.ASCII, 4096, true))
            {
                byte[] buffer = new byte[4096];
                while (true)
                {
                    int bytesRead = source.Read(buffer, 0, buffer.Length);

                    foreach(byte b in buffer)
                    {
                        switch(b)
                        {
                            //case 0x0d:
                            //case 0x0a:
                            //case 0x20:
                            //    writer.Write((char)b);
                            //    break;
                            default:
                                writer.Write(b.ToString("x2"));
                                break;
                        }
                    }

                    if (bytesRead == 0)
                    {
                        writer.Write(">");
                        destination.Flush();
                        return;
                    }
                }
            }
        }
    }
}
