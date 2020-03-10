using System;
using System.Drawing;

namespace FirePDF.Model
{
    internal class Type3Font : Font
    {
        public Type3Font(PdfDictionary dictionary) : base(dictionary)
        {
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

        protected override Cmap LoadEncoding()
        {
            object encodingObj = UnderlyingDict.Get<object>("Encoding");
            if (encodingObj is Name)
            {
                return new Cmap((Name)encodingObj);
            }
            else
            {
                //TODO
                return null;
            }
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
    }
}