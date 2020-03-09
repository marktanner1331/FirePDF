using System;
using System.Collections.Generic;
using System.Drawing;

namespace FirePDF.Model
{
    //pdf spec 9.7.4
    public class CIDType0Font : CIDFont
    {
        private Lazy<CMAP> _encoding;
        public override CMAP encoding => _encoding.Value;

        private Lazy<CMAP> _toUnicode;
        public override CMAP toUnicode => _toUnicode.Value;

        private int defaultWidth
        {
            get => underlyingDict.get<int?>("DW") ?? 1000;
            set => underlyingDict.set("DW", value);
        }

        private Lazy<Dictionary<int, int>> widths;

        private FontDescriptor fontDescriptor
        {
            get => underlyingDict.get<FontDescriptor>("FontDescriptor");
        }

        public CIDType0Font(PDFDictionary dictionary) : base(dictionary)
        {
            widths = new Lazy<Dictionary<int, int>>(parseWidths);
        }

        private Dictionary<int, int> parseWidths()
        {
            if (underlyingDict.containsKey("W"))
            {
                PDFList w = underlyingDict.get<PDFList>("W");
                Dictionary<int, int> widths = new Dictionary<int, int>();

                int counter = 0;
                while (counter < w.count)
                {
                    int first = w.get<int>(counter++);
                    object secondObj = w.get<object>(counter++);

                    if (secondObj is PDFList)
                    {
                        PDFList values = (PDFList)secondObj;

                        for (int i = 0; i < values.count; i++)
                        {
                            widths[first + i] = values.get<int>(i);
                        }
                    }
                    else
                    {
                        int second = (int)secondObj;
                        int width = w.get<int>(counter++);

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

        public override FontDescriptor getFontDescriptor()
        {
            return fontDescriptor;
        }

        /// <summary>
        /// returns the intended width of the cid expressed in user space (before scaling by the text rendering matrix)
        /// </summary>
        public override float getWidthForCID(int cid)
        {
            if (widths.Value.ContainsKey(cid))
            {
                return widths.Value[cid] / 1000f;
            }
            else
            {
                return defaultWidth / 1000f;
            }
        }

        public override SizeF measureText(byte[] hexString, GraphicsState graphicsState)
        {
            throw new NotImplementedException();
        }

        public override string readUnicodeStringFromHexString(byte[] hexString)
        {
            throw new NotImplementedException();
        }

        public override void setToUnicodeCMAP(ObjectReference objectReference)
        {
            throw new NotImplementedException();
        }
    }
}