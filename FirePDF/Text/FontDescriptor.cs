using FirePDF.Model;
using System.Drawing;

namespace FirePDF.Text
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
