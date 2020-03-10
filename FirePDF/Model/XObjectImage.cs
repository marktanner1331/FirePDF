using FirePDF.Reading;
using System.Drawing;
using System.IO;

namespace FirePDF.Model
{
    public class XObjectImage : PdfStream
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
        /// initializing an xObject image with the owning Pdf, its dictionary, and the offset to the start of the stream relative to the start of the Pdf
        /// </summary>
        public XObjectImage(Stream stream, PdfDictionary dict, long startOfStream) : base(stream, dict, startOfStream)
        {
            width = dict.Get<int>("Width");
            height = dict.Get<int>("Height");
        }

        /// <summary>
        /// initializes a new xObject image with the path to an external image file
        /// </summary>
        /// <param name="owner"></param>
        /// <param name="fullPathToImage">the full path to the external image file</param>
        public static XObjectImage FromFile(Pdf owner, string fullPathToImage)
        {
            Image image = Image.FromFile(fullPathToImage);
            PdfDictionary dictionary = new PdfDictionary(owner);
            dictionary.Set("Width", image.Width);
            dictionary.Set("Height", image.Height);
            dictionary.Set("Type", (Name)"XObject");
            dictionary.Set("Subtype", (Name)"Image");
            dictionary.Set("Filter", (Name)"DCTDecode");

            using (FileStream stream = File.OpenRead(fullPathToImage))
            {
                dictionary.Set("Length", stream.Length);

                ObjectReference objectRef = owner.store.AddStream(stream, dictionary);
                return objectRef.Get<XObjectImage>();
            }
        }

        /// <summary>
        /// returns the image as a System.Drawing.Bitmap
        /// </summary>
        /// <returns>the image</returns>
        public Bitmap GetImage()
        {
            stream.Position = startOfStream;
            Bitmap image = PdfReader.DecompressImageStream(Pdf, stream, UnderlyingDict);

            if(UnderlyingDict.ContainsKey("SMask"))
            {
                XObjectImage mask = UnderlyingDict.Get<XObjectImage>("SMask");
                //DoApplyMask(image, mask);
            }

            return image;
        }

    }
}
