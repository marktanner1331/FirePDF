using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FirePDF.Model
{
    public class FontDescriptor
    {
        public readonly RectangleF bbox;

        public FontDescriptor(PDFDictionary dictionary)
        {
            bbox = dictionary.get<PDFList>("FontBBox").asRectangle();
        }
    }
}
