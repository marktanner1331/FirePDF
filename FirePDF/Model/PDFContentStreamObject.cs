using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FirePDF.Model
{
    public abstract class PDFContentStreamObject
    {
        protected readonly PDF pdf;
        public readonly Dictionary<string, object> streamDictionary;

        public abstract Stream readStream();

        public PDFContentStreamObject(PDF pdf, Dictionary<string, object> streamDictionary)
        {
            this.pdf = pdf;
            this.streamDictionary = streamDictionary;
        }
    }
}
