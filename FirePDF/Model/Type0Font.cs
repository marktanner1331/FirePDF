using System;
using System.Drawing;
using System.IO;
using System.Text;

namespace FirePDF.Model
{
    //Pdf spec 9.7
    public class Type0Font : Font
    {
        private CidFont DescendantFont => (CidFont)UnderlyingDict.Get<PdfList>("DescendantFonts").Get<Font>(0);

        public Type0Font(PdfDictionary dictionary) : base(dictionary)
        {
            
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

        public override FontDescriptor GetFontDescriptor()
        {
            return DescendantFont.GetFontDescriptor();
        }

        public override SizeF MeasureText(byte[] hexString, GraphicsState gs)
        {
            FontDescriptor fontDescriptor = GetFontDescriptor();

            //bbox is in glyph space, but we are expressing everything in user space here
            float height = fontDescriptor.bbox.Height / 1000 * gs.fontSize;

            SizeF size = new SizeF(0, height);

            using (MemoryStream stream = new MemoryStream(hexString))
            {
                while (stream.Position != stream.Length)
                {
                    int code = Encoding.ReadCodeFromStream(stream);
                    int cid = Encoding.CodeToCid(code);

                    //hint: the char spacing and word spacing are scaled by the horizontal scaling but not the font size
                    
                    float width = DescendantFont.GetWidthForCid(cid);
                    size.Width += width * gs.fontSize;

                    size.Width += gs.characterSpacing;

                    if(code == 32)
                    {
                        size.Width += gs.wordSpacing;
                    }
                }
            }

            size.Width *= gs.horizontalScaling;

            size.Width *= gs.textMatrix.Elements[0];
            size.Height *= gs.textMatrix.Elements[3];
            
            return size;
        }

        public override string ReadUnicodeStringFromHexString(byte[] hexString)
        {
            if (ToUnicode == null)
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
            UnderlyingDict.Set("ToUnicode", objectReference);
        }
    }
}
