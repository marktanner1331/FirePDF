using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace FirePDF.Model
{
    public class Pattern : PdfStream, IPdfMetaData
    {
        public Pattern(Stream stream, PdfDictionary dictionary, long startOfStream) : base(stream, dictionary, startOfStream)
        {

        }
    }
}
