using FirePDF.Reading;
using System;
using System.Drawing;
using System.IO;
using System.Text;

namespace FirePDF.Model
{
    internal class TrueTypeFont : Font
    {
        public TrueTypeFont(PDFDictionary dictionary) : base(dictionary)
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
            if (toUnicode == null || encoding == null)
            {
                return "";
            }

            using (MemoryStream stream = new MemoryStream(hexString))
            {
                StringBuilder sb = new StringBuilder();
                while (stream.Position != stream.Length)
                {
                    int code = encoding.readCodeFromStream(stream);
                    string str = toUnicode.codeToUnicode(code);

                    sb.Append(str);
                }

                return sb.ToString();
            }
        }

        public override void setToUnicodeCMAP(ObjectReference objectReference)
        {
            throw new NotImplementedException();
        }
    }
}