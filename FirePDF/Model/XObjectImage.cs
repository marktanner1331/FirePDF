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
    public class XObjectImage : PDFStream
    {
        /// <summary>
        /// the natural width of the image
        /// </summary>
        public readonly int width;

        /// <summary>
        /// the natural height of the image
        /// </summary>
        public readonly int height;

        /// <summary>
        /// initializing an xobject image with the owning pdf, its dictionary, and the offset to the start of the stream relative to the start of the pdf
        /// </summary>
        public XObjectImage(PDF pdf, Stream stream, PDFDictionary dict, long startOfStream) : base(pdf, stream, dict, startOfStream)
        {
            width = dict.get<int>("Width");
            height = dict.get<int>("Height");
        }

        public Bitmap getImage()
        {
            stream.Position = startOfStream;
            Bitmap image = PDFReader.decompressImageStream(pdf, stream, underlyingDict);

            if(underlyingDict.ContainsKey("SMask"))
            {
                XObjectImage mask = underlyingDict.get<XObjectImage>("SMask");
                //DoApplyMask(image, mask);
            }

            return image;
        }

    }
}
