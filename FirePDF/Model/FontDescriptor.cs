using System.Drawing;

namespace FirePDF.Model
{
    public class FontDescriptor : HaveUnderlyingDict
    {
        public readonly RectangleF bbox;

        public FontDescriptor(PdfDictionary dictionary) : base(dictionary)
        {
            bbox = dictionary.Get<PdfList>("FontBBox").AsRectangle();
        }
    }
}
