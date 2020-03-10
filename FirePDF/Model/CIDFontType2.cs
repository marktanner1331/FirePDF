using System;
using System.Drawing;

namespace FirePDF.Model
{
    internal class CIDFontType2 : Font
    {
        public CIDFontType2(PDFDictionary dictionary) : base(dictionary)
        {
        }

        protected override CMAP loadEncoding()
        {
            object encodingObj = underlyingDict.get<object>("Encoding");
            if (encodingObj is Name)
            {
                return new CMAP((Name)encodingObj);
            }
            else
            {
                //TODO
                return null;
            }
        }

        protected override CMAP loadToUnicode()
        {
            if (underlyingDict.containsKey("ToUnicode"))
            {
                PDFStream stream = underlyingDict.get<PDFStream>("ToUnicode");

                if (stream.underlyingDict.containsKey("UseCMap"))
                {
                    //in theory we just load the other cmap and merge it with this one
                    throw new NotImplementedException();
                }

                return new CMAP(stream.getDecompressedStream());
            }
            else
            {
                return null;
            }
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