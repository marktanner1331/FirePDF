using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FirePDF
{
    class FlateContentStream : PDFContentStream
    {
        private long startOfStream;

        /// <summary>
        /// represents a flate stream, providing methods for decoding
        /// </summary>
        /// <param name="pdf"></param>
        /// <param name="dict">the stream dictionary</param>
        /// <param name="startOfStream">the offset of the 'stream' keywords</param>
        public FlateContentStream(PDF pdf, Dictionary<string, object> dict, long startOfStream) : base(pdf, dict)
        {
            this.startOfStream = startOfStream;
        }

        /// <summary>
        /// returns a seekable stream
        /// </summary>
        public override Stream readStream()
        {
            pdf.stream.Position = startOfStream;
            string chunk = ASCIIReader.readASCIIString(pdf.stream, 6);
            if (chunk != "stream")
            {
                throw new Exception();
            }

            PDFObjectReader.skipOverWhiteSpace(pdf.stream);

            //http://george.chiramattel.com/blog/2007/09/deflatestream-block-length-does-not-match.html
            pdf.stream.Position += 2;

            int length = (int)streamDictionary["Length"] - 2;

            byte[] buffer = new byte[length];
            pdf.stream.Read(buffer, 0, length);

            PDFObjectReader.skipOverWhiteSpace(pdf.stream);
            chunk = ASCIIReader.readASCIIString(pdf.stream, 9);
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
