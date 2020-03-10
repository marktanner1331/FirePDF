using System;
using System.Drawing;

namespace FirePDF.Model
{
    internal class Type3Font : Font
    {
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
    }
}