using System;
using System.Drawing;

namespace FirePDF.Model
{
    internal class CidFontType2 : Font
    {
        public CidFontType2(PdfDictionary dictionary) : base(dictionary)
        {
        }

        protected override Cmap LoadEncoding()
        {
            object encodingObj = UnderlyingDict.Get<object>("Encoding");
            return encodingObj is Name name ? new Cmap(name) : null;
        }

        protected override Cmap LoadToUnicode()
        {
            if (UnderlyingDict.ContainsKey("ToUnicode"))
            {
                PdfStream stream = UnderlyingDict.Get<PdfStream>("ToUnicode");

                if (stream.UnderlyingDict.ContainsKey("UseCMap"))
                {
                    //in theory we just load the other cmap and merge it with this one
                    throw new NotImplementedException();
                }

                return new Cmap(stream.GetDecompressedStream());
            }
            else
            {
                return null;
            }
        }

        public override FontDescriptor GetFontDescriptor()
        {
            throw new NotImplementedException();
        }

        public override SizeF MeasureText(byte[] hexString, GraphicsState graphicsState)
        {
            throw new NotImplementedException();
        }

        public override string ReadUnicodeStringFromHexString(byte[] hexString)
        {
            throw new NotImplementedException();
        }

        public override void SetToUnicodeCmap(ObjectReference objectReference)
        {
            throw new NotImplementedException();
        }
    }
}