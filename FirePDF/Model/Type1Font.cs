using System;
using System.Drawing;
using System.IO;
using System.Text;

namespace FirePDF.Model
{
    public class Type1Font : Font
    {
        public Type1Font(PdfDictionary dictionary) : base(dictionary)
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
            if (ToUnicode == null || Encoding == null)
            {
                return "";
            }

            using (MemoryStream stream = new MemoryStream(hexString))
            {
                StringBuilder sb = new StringBuilder();
                while (stream.Position != stream.Length)
                {
                    int code = Encoding.ReadCodeFromStream(stream);
                    string str = ToUnicode.CodeToUnicode(code);

                    sb.Append(str);
                }

                return sb.ToString();
            }
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