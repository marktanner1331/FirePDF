using System;
using System.Drawing;

namespace FirePDF.Model
{
    internal class Type3Font : Font
    {
        private Lazy<CMAP> _encoding;
        public override CMAP encoding => _encoding.Value;

        private Lazy<CMAP> _toUnicode;
        public override CMAP toUnicode => _toUnicode.Value;

        public Type3Font(PDFDictionary dictionary) : base(dictionary)
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

        public override void setToUnicodeCMAP(ObjectReference objectReference)
        {
            throw new NotImplementedException();
        }
    }
}