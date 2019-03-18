using FirePDF.Model;
using FirePDF.Reading;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FirePDF.StreamHelpers
{
    class FlateContentStream
    {
        /// <summary>
        /// decompresses a stream from the pdf stream at the current position and returns it
        /// </summary>
        public static Stream decompressStream(Stream pdfStream, Dictionary<string, object> streamDictionary)
        {
            //http://george.chiramattel.com/blog/2007/09/deflatestream-block-length-does-not-match.html
            pdfStream.Position += 2;

            int length = (int)streamDictionary["Length"] - 2;

            byte[] buffer = new byte[length];
            pdfStream.Read(buffer, 0, length);

            PDFReader.skipOverWhiteSpace(pdfStream);
            string chunk = ASCIIReader.readASCIIString(pdfStream, 9);
            if (chunk != "endstream")
            {
                throw new Exception();
            }

            using (DeflateStream decompressionStream = new DeflateStream(new MemoryStream(buffer), CompressionMode.Decompress))
            {
                MemoryStream decompressed = new MemoryStream();
                decompressionStream.CopyTo(decompressed);

                if (streamDictionary.ContainsKey("DecodeParms") == false)
                {
                    decompressed.Seek(0, SeekOrigin.Begin);
                    return decompressed;
                }
                else
                {
                    int columns = (int)((Dictionary<string, object>)streamDictionary["DecodeParms"])["Columns"];

                    byte[] predictedBytes = decompressed.ToArray();
                    byte[] plainBytes = PNGPredictor.decompress(predictedBytes, columns);

                    decompressed.Dispose();
                    return new MemoryStream(plainBytes);
                }
            }
        }
    }
}
