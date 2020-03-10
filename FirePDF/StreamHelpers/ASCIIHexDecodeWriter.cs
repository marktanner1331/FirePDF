using System.IO;
using System.Text;

namespace FirePDF.StreamHelpers
{
    public static class AsciiHexDecodeWriter
    {
        public static void Encode(Stream source, Stream destination)
        {
            //not disposing the writer on purpose so the stream stays open
            StreamWriter writer = new StreamWriter(destination);

            byte[] buffer = new byte[4096];
            while (true)
            {
                int bytesRead = source.Read(buffer, 0, buffer.Length);

                foreach (byte b in buffer)
                {
                    switch (b)
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
