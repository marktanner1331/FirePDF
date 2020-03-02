using System.Drawing;

namespace FirePDF.Model
{
    class Type1Font : Font
    {
        public Type1Font(PDFDictionary dictionary) : base(dictionary)
        {
        }

        public override FontDescriptor getFontDescriptor()
        {
            throw new System.NotImplementedException();
        }

        public override SizeF measureText(byte[] hexString, GraphicsState graphicsState)
        {
            throw new System.NotImplementedException();
        }

        public override string readUnicodeStringFromHexString(byte[] hexString)
        {
            throw new System.NotImplementedException();
        }
    }
}