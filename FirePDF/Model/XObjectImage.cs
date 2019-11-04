using FirePDF.Reading;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FirePDF.Model
{
    public class XObjectImage
    {
        private PDF pdf;
        public Dictionary<Name, object> underlyingDict { get; private set; }
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
        public XObjectImage(PDF pdf, Dictionary<Name, object> dict, long startOfStream)
        {
            this.pdf = pdf;
            this.underlyingDict = dict;
            this.startOfStream = startOfStream;
        }

        public Bitmap getImage()
        {
            pdf.stream.Position = startOfStream;
            Bitmap image = PDFReader.decompressImageStream(pdf, pdf.stream, underlyingDict);

            if(underlyingDict.ContainsKey("SMask"))
            {
                XObjectImage mask = (XObjectImage)PDFReader.readIndirectObject(pdf, (ObjectReference)underlyingDict["SMask"]);
                //DoApplyMask(image, mask);
            }

            return image;
        }

        public Stream getRawStream()
        {
            pdf.stream.Position = startOfStream;
            return PDFReader.decompressStream(pdf, pdf.stream, underlyingDict);
        }
    }
}
