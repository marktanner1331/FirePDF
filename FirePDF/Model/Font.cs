using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FirePDF.Model
{
    public abstract class Font
    {
        private PDF pdf;
        private PDFDictionary dictionary;

        protected Font(PDF pdf, PDFDictionary dictionary)
        {
            this.pdf = pdf;
            this.dictionary = dictionary;
        }

        public static Font loadExistingFontFromPDF(PDF pdf, PDFDictionary dictionary)
        {
            Name subType = dictionary.get<Name>("Subtype");
            switch(subType)
            {
                case "Type0":
                    return new Type0Font(pdf, dictionary);
                case "CIDFontType0":
                    return new CIDType0Font(pdf, dictionary);
                default:
                    throw new NotImplementedException();
            }
        }

        /// <summary>
        /// converts a hex string to unicode
        /// this is not always possible, and this method can return an empty string in that case
        /// </summary>
        public abstract string readUnicodeStringFromHexString(byte[] hexString);

        public abstract FontDescriptor getFontDescriptor();

        /// <summary>
        /// returns the size of the text
        /// including char spacing, word spacing, and scaling by the horizontal scaling, font size, text matrix
        /// but not the current transformation matrix
        /// </summary>
        public abstract SizeF measureText(byte[] hexString, GraphicsState graphicsState);
    }
}
