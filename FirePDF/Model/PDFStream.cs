using FirePDF.Reading;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FirePDF.Model
{
    /// <summary>
    /// represents any pdf object that includes a stream, including forms and images
    /// </summary>
    public class PDFStream
    {
        protected readonly PDF pdf;
        public readonly PDFDictionary underlyingDict;
        protected readonly long startOfStream;

        public PDFStream(PDF pdf, PDFDictionary underlyingDictionary, long startOfStream)
        {
            this.pdf = pdf;
            this.underlyingDict = underlyingDictionary;
            this.startOfStream = startOfStream;
        }
        
        public Stream getRawStream()
        {
            pdf.stream.Position = startOfStream;
            return PDFReader.decompressStream(pdf, pdf.stream, underlyingDict);
        }
    }
}
