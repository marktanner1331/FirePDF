using FirePDF.Reading;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FirePDF.Model
{
    public class XObjectImage
    {
        private PDF pdf;
        private Dictionary<string, object> underlyingDict;
        public long startOfStream;

        public XObjectImage(PDF pdf)
        {
            this.pdf = pdf;

            //need to initialize a new underlyingDict and stream
            throw new NotImplementedException();
        }

        /// <summary>
        /// initializing an xobject image with the owning pdf, its dictionary, and the offset to the start of the stream relative to the start of the pdf
        /// </summary>
        public XObjectImage(PDF pdf, Dictionary<string, object> dict, long startOfStream)
        {
            this.pdf = pdf;
            this.underlyingDict = dict;
            this.startOfStream = startOfStream;
        }

        public Image getImage()
        {
            MemoryStream temp = new MemoryStream();
            pdf.stream.Position = startOfStream;

            string chunk = ASCIIReader.readASCIIString(pdf.stream, 6);
            if (chunk != "stream")
            {
                throw new Exception();
            }

            PDFReader.skipOverWhiteSpace(pdf.stream);

            long length = (int)underlyingDict["Length"];

            byte[] buffer = new byte[4096];
            while(length > 0)
            {
                int bytesToRead = (int)Math.Min(buffer.Length, length);
                int bytesRead = pdf.stream.Read(buffer, 0, bytesToRead);

                temp.Write(buffer, 0, bytesRead);

                length -= bytesRead;
            }

            temp.Seek(0, SeekOrigin.Begin);

            return Image.FromStream(temp, true, true);
        }
    }
}
