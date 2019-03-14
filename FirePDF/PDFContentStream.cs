using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FirePDF
{
    public abstract class PDFContentStream
    {
        protected readonly PDF pdf;
        public readonly Dictionary<string, object> streamDictionary;

        public abstract Stream readStream();

        public PDFContentStream(PDF pdf, Dictionary<string, object> streamDictionary)
        {
            this.pdf = pdf;
            this.streamDictionary = streamDictionary;
        }
    }
}
