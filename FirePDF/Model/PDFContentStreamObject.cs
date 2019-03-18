using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FirePDF.Model
{
    /// <summary>
    /// a class that wraps a content stream object, i.e. page stream or xobject stream and provides methods for reading it
    /// </summary>
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
