using System;
using System.IO;
using FirePDF.Model;

namespace FirePDF.Text
{
    public class PDFEncoding : HaveUnderlyingDict
    {
        private Cmap cmap;

        public PDFEncoding(PdfDictionary underlyingDict) : base(underlyingDict)
        {
            cmap = null;
        }

        public PDFEncoding(Cmap cmap)
        {
            this.cmap = cmap;
        }

        internal void WriteToStream(MemoryStream stream)
        {
            if(cmap != null)
            {
                cmap.WriteToStream(stream);
            }
            else
            {
                throw new NotImplementedException();
            }
        }

        public int ReadCodeFromStream(MemoryStream stream)
        {
            if (cmap != null)
            {
                return cmap.ReadCodeFromStream(stream);
            }
            else
            {
                return stream.ReadByte();
            }
        }

        internal int CodeToCid(int code)
        {
            if(cmap != null)
            {
                return cmap.CodeToCid(code);
            }
            else
            {
                throw new NotImplementedException();
            }
        }
    }
}