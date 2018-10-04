using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FirePDF
{
    class FlateContentStream : PDFContentStream
    {
        private PDF pdf;
        private Dictionary<string, object> dict;
        private long startOfStream;

        /// <summary>
        /// represents a flate stream, providing methods for decoding
        /// </summary>
        /// <param name="pdf"></param>
        /// <param name="dict">the stream dictionary</param>
        /// <param name="startOfStream">the offset of the 'stream' keywords</param>
        public FlateContentStream(PDF pdf, Dictionary<string, object> dict, long startOfStream)
        {
            this.pdf = pdf;
            this.dict = dict;
            this.startOfStream = startOfStream;
        }
    }
}
