using FirePDF.Reading;
using System;
using System.Drawing;
using System.IO;
using System.Text;

namespace FirePDF.Model
{
    internal class TrueTypeFont : Font
    {
        private Lazy<CMAP> _encoding;
        public override CMAP encoding => _encoding.Value;

        private Lazy<CMAP> _toUnicode;
        public override CMAP toUnicode => _toUnicode.Value;

        public TrueTypeFont(PDFDictionary dictionary) : base(dictionary)
        {
            _encoding = new Lazy<CMAP>(() =>
            {
                object encodingObj = underlyingDict.get<object>("Encoding");
                if (encodingObj is Name)
                {
                    return new CMAP((Name)encodingObj);
                }
                else
                {
                    //TODO
                    //encoding stored in the font
                    return null;
                }
            });

            _toUnicode = new Lazy<CMAP>(() =>
            {
                if (dictionary.containsKey("ToUnicode"))
                {
                    PDFStream stream = dictionary.get<PDFStream>("ToUnicode");

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
            });
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