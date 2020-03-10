namespace FirePDF.Model
{
    internal class PdfMetaDataDictionary : HaveUnderlyingDict, IPdfMetaData
    {
        public PdfMetaDataDictionary(PdfDictionary dictionary) : base(dictionary)
        {
            
        }
    }
}