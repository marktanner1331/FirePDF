using System;
using System.Collections.Generic;
using System.Drawing;

namespace FirePDF.Model
{
    //Pdf spec 9.7.4
    public class CidType0Font : CidFont
    {
        private int DefaultWidth
        {
            get => UnderlyingDict.Get<int?>("DW") ?? 1000;
            set => UnderlyingDict.Set("DW", value);
        }

        private readonly Lazy<Dictionary<int, int>> widths;

        private FontDescriptor FontDescriptor => UnderlyingDict.Get<FontDescriptor>("FontDescriptor");

        protected override Cmap LoadEncoding()
        {
            object encodingObj = UnderlyingDict.Get<object>("Encoding");
            if (encodingObj is Name name)
            {
                return new Cmap(name);
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

        public CidType0Font(PdfDictionary dictionary) : base(dictionary)
        {
            widths = new Lazy<Dictionary<int, int>>(ParseWidths);
        }

        private Dictionary<int, int> ParseWidths()
        {
            if (UnderlyingDict.ContainsKey("W"))
            {
                PdfList w = UnderlyingDict.Get<PdfList>("W");
                Dictionary<int, int> widths = new Dictionary<int, int>();

                int counter = 0;
                while (counter < w.Count)
                {
                    int first = w.Get<int>(counter++);
                    object secondObj = w.Get<object>(counter++);

                    if (secondObj is PdfList values)
                    {
                        for (int i = 0; i < values.Count; i++)
                        {
                            widths[first + i] = values.Get<int>(i);
                        }
                    }
                    else
                    {
                        int second = (int)secondObj;
                        int width = w.Get<int>(counter++);

                        for (int i = first; i <= second; i++)
                        {
                            widths[i] = width;
                        }
                    }
                }

                return widths;
            }
            else
            {
                return new Dictionary<int, int>();
            }
        }

        public override FontDescriptor GetFontDescriptor()
        {
            return FontDescriptor;
        }

        /// <summary>
        /// returns the intended width of the cid expressed in user space (before scaling by the text rendering matrix)
        /// </summary>
        public override float GetWidthForCid(int cid)
        {
            if (widths.Value.ContainsKey(cid))
            {
                return widths.Value[cid] / 1000f;
            }
            else
            {
                return DefaultWidth / 1000f;
            }
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