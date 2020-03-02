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
        public XObjectImage(Stream stream, PDFDictionary dict, long startOfStream) : base(stream, dict, startOfStream)
        {
            width = dict.get<int>("Width");
            height = dict.get<int>("Height");
        }

        /// <summary>
        /// initializes a new xObject image with the path to an external image file
        /// </summary>
        /// <param name="owner"></param>
        /// <param name="fullPathToImage">the full path to the external image file</param>
        public static XObjectImage fromFile(PDF owner, string fullPathToImage)
        {
            Image image = Image.FromFile(fullPathToImage);
            PDFDictionary dictionary = new PDFDictionary(owner);
            dictionary.set("Width", image.Width);
            dictionary.set("Height", image.Height);
            dictionary.set("Type", (Name)"XObject");
            dictionary.set("Subtype", (Name)"Image");
            dictionary.set("Filter", (Name)"DCTDecode");

            using (FileStream stream = File.OpenRead(fullPathToImage))
            {
                dictionary.set("Length", stream.Length);

                ObjectReference objectRef = owner.store.addStream(stream, dictionary);
                return objectRef.get<XObjectImage>();
            }
        }

        /// <summary>
        /// returns the image as a System.Drawing.Bitmap
        /// </summary>
        /// <returns>the image</returns>
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
