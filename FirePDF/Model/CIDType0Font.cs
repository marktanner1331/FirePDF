using System;
using System.Collections.Generic;
using System.Drawing;

namespace FirePDF.Model
{
    //pdf spec 9.7.4
    public class CIDType0Font : CIDFont
    {
        private int defaultWidth = 1000;
        private Dictionary<int, int> widths = new Dictionary<int, int>();
        private FontDescriptor fontDescriptor;

        public CIDType0Font(PDF pdf, PDFDictionary dictionary) : base(pdf, dictionary)
        {
            if (dictionary.ContainsKey("DW"))
            {
                defaultWidth = dictionary.get<int>("DW");
            }

            if (dictionary.ContainsKey("W"))
            {
                PDFList w = dictionary.get<PDFList>("W");

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
            }

            fontDescriptor = new FontDescriptor(dictionary.get<PDFDictionary>("FontDescriptor"));
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
            if(widths.ContainsKey(cid))
            {
                return widths[cid] / 1000f;
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
    }
}