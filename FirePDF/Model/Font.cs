using System;
using System.Drawing;
using System.IO;

namespace FirePDF.Model
{
    public abstract class Font : HaveUnderlyingDict
    {
        private readonly Lazy<Cmap> encoding;
        public Cmap Encoding => encoding.Value;

        private readonly Lazy<Cmap> toUnicode;
        public Cmap ToUnicode => toUnicode.Value;

        public Name baseFont;

        protected Font(PdfDictionary dictionary) : base(dictionary)
        {
            baseFont = dictionary.Get<Name>("BaseFont");
            encoding = new Lazy<Cmap>(LoadEncoding);
            toUnicode = new Lazy<Cmap>(LoadToUnicode);
        }

        protected abstract Cmap LoadEncoding();

        protected abstract Cmap LoadToUnicode();


        public override bool IsDirty()
        {
            if (encoding.IsValueCreated && Encoding != null && Encoding.IsDirty)
            {
                return true;
            }

            if (toUnicode.IsValueCreated && ToUnicode != null && ToUnicode.IsDirty)
            {
                return true;
            }

            return base.IsDirty();
        }

        public static Font LoadExistingFontFromPdf(PdfDictionary dictionary)
        {
            Name subType = dictionary.Get<Name>("Subtype");
            switch (subType)
            {
                case "Type0":
                    return new Type0Font(dictionary);
                case "Type1":
                    return new Type1Font(dictionary);
                case "Type3":
                    return new Type3Font(dictionary);
                case "CIDFontType0":
                    return new CidType0Font(dictionary);
                case "CIDFontType2":
                    return new CidFontType2(dictionary);
                case "TrueType":
                    return new TrueTypeFont(dictionary);
                default:
                    throw new NotImplementedException();
            }
        }

        public abstract void SetToUnicodeCmap(ObjectReference objectReference);

        /// <summary>
        /// converts a Pdf hex string shown by the Tj or TJ operator into a unicode string
        /// this is not always possible, and this method can return an empty string in that case
        /// </summary>
        //TODO move implementation into this class
        //like the above method
        public abstract string ReadUnicodeStringFromHexString(byte[] hexString);

        public abstract FontDescriptor GetFontDescriptor();

        /// <summary>
        /// returns the size of the text
        /// including char spacing, word spacing, and scaling by the horizontal scaling, font size, text matrix
        /// but not the current transformation matrix
        /// </summary>
        public abstract SizeF MeasureText(byte[] hexString, GraphicsState graphicsState);

        internal void PrepareForWriting()
        {
            if(encoding.IsValueCreated && Encoding != null && Encoding.IsDirty)
            {
                using (MemoryStream stream = new MemoryStream())
                {
                    Encoding.WriteToStream(stream);
                }
            }
        }
    }
}
