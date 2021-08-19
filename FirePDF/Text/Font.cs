using FirePDF.Model;
using System;
using System.Drawing;
using System.IO;

namespace FirePDF.Text
{
    public abstract class Font : HaveUnderlyingDict
    {
        private readonly Lazy<PDFEncoding> encoding;
        public PDFEncoding Encoding => encoding.Value;

        private readonly Lazy<Cmap> toUnicode;
        public Cmap ToUnicode => toUnicode.Value;

        public Name baseFont;

        protected Font(PdfDictionary dictionary) : base(dictionary)
        {
            baseFont = dictionary.Get<Name>("BaseFont");
            encoding = new Lazy<PDFEncoding>(LoadEncoding);
            toUnicode = new Lazy<Cmap>(LoadToUnicode);
        }

        protected virtual PDFEncoding LoadEncoding()
        {
            object encodingObj = UnderlyingDict.Get("Encoding", true);

            if(encodingObj is null)
            {
                return null;
            }
            else if (encodingObj is Name encodingName)
            {
                return new PDFEncoding(new Cmap(encodingName));
            }
            else if(encodingObj is PDFEncoding encodingDict)
            {
                //TODO move to PdfEncoding
                return encodingDict;

                //if(encodingDict.UnderlyingDict.ContainsKey("BaseEncoding") == false)
                //{
                //    throw new Exception("use the font's built-in encoding");
                //}

                //Name baseEncoding = encodingDict.UnderlyingDict.Get<Name>("BaseEncoding");
                //Cmap cmap = new Cmap(baseEncoding);

                //if (encodingDict.UnderlyingDict.ContainsKey("Differences"))
                //{
                //    PdfList differences = encodingDict.UnderlyingDict.Get<PdfList>("Differences");
                //    //cmap.addDifferences(differences);
                //}

                //return cmap;
            }
            else if(encodingObj is PdfDictionary dict)
            {
                return new PDFEncoding(dict);
            }
            else
            {
                throw new Exception();
            }
        }

        protected abstract Cmap LoadToUnicode();


        public override bool IsDirty()
        {
            if (Encoding != null && encoding.IsValueCreated && Encoding.UnderlyingDict != null && Encoding.IsDirty())
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
                case "Type1C":
                    return new Type1CFont(dictionary);
                case "CIDFontType0C":
                    return new CIDFontType0C(dictionary);
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

        public string ReadUnicodeStringFromHexString(PdfString s) => ReadUnicodeStringFromHexString(s.ToByteArray());

        public abstract FontDescriptor GetFontDescriptor();

        /// <summary>
        /// returns the size of the text
        /// including char spacing, word spacing, and scaling by the horizontal scaling, font size, text matrix
        /// but not the current transformation matrix
        /// </summary>
        public abstract SizeF MeasureText(byte[] hexString, GraphicsState graphicsState);

        internal void PrepareForWriting()
        {
            if(encoding.IsValueCreated && Encoding != null && Encoding.IsDirty())
            {
                using (MemoryStream stream = new MemoryStream())
                {
                    Encoding.WriteToStream(stream);
                }
            }
        }
    }
}
