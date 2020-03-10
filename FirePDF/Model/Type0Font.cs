using FirePDF.Reading;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FirePDF.Model
{
    //pdf spec 9.7
    public class Type0Font : Font
    {
        private CIDFont descendantFont
        {
            get => (CIDFont)underlyingDict.get<PDFList>("DescendantFonts").get<Font>(0);
        }

        public Type0Font(PDFDictionary dictionary) : base(dictionary)
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
            return descendantFont.getFontDescriptor();
        }

        public override SizeF measureText(byte[] hexString, GraphicsState gs)
        {
            FontDescriptor fontDescriptor = getFontDescriptor();

            //bbox is in glyph space, but we are expressing everything in user space here
            float height = fontDescriptor.bbox.Height / 1000 * gs.fontSize;

            SizeF size = new SizeF(0, height);

            using (MemoryStream stream = new MemoryStream(hexString))
            {
                while (stream.Position != stream.Length)
                {
                    int code = encoding.readCodeFromStream(stream);
                    int cid = encoding.codeToCID(code);

                    //hint: the char spacing and word spacing are scaled by the horizontal scaling but not the font size
                    
                    float width = descendantFont.getWidthForCID(cid);
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

        public override string readUnicodeStringFromHexString(byte[] hexString)
        {
            if (toUnicode == null)
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
            underlyingDict.set("ToUnicode", objectReference);
        }
    }
}
