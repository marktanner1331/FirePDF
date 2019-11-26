using System.IO;

namespace FirePDF.Model
{
    internal class PDFMetaData : PDFStream
    {
        public PDFMetaData(Stream stream, PDFDictionary underlyingDictionary, long startOfStream) : base(stream, underlyingDictionary, startOfStream)
        {
        }
    }
}