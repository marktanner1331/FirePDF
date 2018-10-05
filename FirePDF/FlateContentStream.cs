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
        private PDF pdf;
        private Dictionary<string, object> dict;
        private long startOfStream;

        /// <summary>
        /// represents a flate stream, providing methods for decoding
        /// </summary>
        /// <param name="pdf"></param>
        /// <param name="dict">the stream dictionary</param>
        /// <param name="startOfStream">the offset of the 'stream' keywords</param>
        public FlateContentStream(PDF pdf, Dictionary<string, object> dict, long startOfStream)
        {
            this.pdf = pdf;
            this.dict = dict;
            this.startOfStream = startOfStream;
        }

        public override Stream readStream()
        {
            pdf.stream.Position = startOfStream;
            string chunk = FileReader.readASCIIString(pdf.stream, 6);
            if(chunk != "stream")
            {
                throw new Exception();
            }

            PDFObjectReader.skipOverWhiteSpace(pdf.stream);

            //http://george.chiramattel.com/blog/2007/09/deflatestream-block-length-does-not-match.html
            pdf.stream.Position += 2;

            int length = (int)dict["Length"];
            byte[] buffer = new byte[length];
            pdf.stream.Read(buffer, 0, length);
            
            PDFObjectReader.skipOverWhiteSpace(pdf.stream);
            chunk = FileReader.readASCIIString(pdf.stream, 9);
            if (chunk != "endstream")
            {
                throw new Exception();
            }

            return new DeflateStream(new MemoryStream(buffer), CompressionMode.Decompress);
        }
    }
}
