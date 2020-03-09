using FirePDF.Writing;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FirePDF.Model
{
    public abstract class Font :IHaveUnderlyingDict
    {
        public Name baseFont;
        public abstract CMAP encoding { get; }

        public abstract CMAP toUnicode { get; }

        protected Font(PDFDictionary dictionary) : base(dictionary)
        {
            baseFont = dictionary.get<Name>("BaseFont");
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
                case "Type3":
                    return new Type3Font(dictionary);
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

        public abstract void setToUnicodeCMAP(ObjectReference objectReference);
        
        /// <summary>
        /// converts a pdf hex string shown by the Tj or TJ operator into a unicode string
        /// this is not always possible, and this method can return an empty string in that case
        /// </summary>
        //TODO move implementation into this class
        //like the above method
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
