using System;
using System.IO;

namespace FirePDF.Model
{
    internal class PDFMetaDataStream : PDFStream, PDFMetaData
    {
        public PDFMetaDataStream(Stream stream, PDFDictionary dictionary, long startOfStream) : base(stream, dictionary, startOfStream)
        {

        }
    }
}