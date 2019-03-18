using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FirePDF.StreamHelpers
{
    public class RawContentStream
    {
        /// <summary>
        /// decompresses a stream from the pdf stream at the current position and returns it
        /// as the stream isn't compressed, this method has the effect of just copying the bytes to a new stream
        /// </summary>
        public static Stream decompressStream(Stream pdfStream, Dictionary<string, object> streamDictionary)
        {
            MemoryStream temp = new MemoryStream();

            long length = (int)streamDictionary["Length"];

            byte[] buffer = new byte[4096];
            while (length > 0)
            {
                int bytesToRead = (int)Math.Min(buffer.Length, length);
                int bytesRead = pdfStream.Read(buffer, 0, bytesToRead);

                temp.Write(buffer, 0, bytesRead);

                length -= bytesRead;
            }

            temp.Seek(0, SeekOrigin.Begin);

            return temp;
        }
    }
}
