using System.IO;

namespace FirePDF.Model
{
    internal class PdfMetaDataStream : PdfStream, IPdfMetaData
    {
        public PdfMetaDataStream(Stream stream, PdfDictionary dictionary, long startOfStream) : base(stream, dictionary, startOfStream)
        {

        }
    }
}