using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FirePDF.Model
{
    public abstract class Font :IHaveUnderlyingDict
    {
        protected Font(PDFDictionary dictionary) : base(dictionary)
        {
        }

        public static Font loadExistingFontFromPDF(PDFDictionary dictionary)
        {
            Name subType = dictionary.get<Name>("Subtype");
            switch(subType)
            {
                case "Type0":
                    return new Type0Font(dictionary);
                case "Type1":
                    return new Type1Font(dictionary);
                case "CIDFontType0":
                    return new CIDType0Font(dictionary);
                case "CIDFontType2":
                    return new CIDFontType2(dictionary);
                case "TrueType":
                    return new TrueTypeFont(dictionary);
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
