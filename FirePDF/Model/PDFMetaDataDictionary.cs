using System;
using System.IO;

namespace FirePDF.Model
{
    internal class PDFMetaDataDictionary : IHaveUnderlyingDict, PDFMetaData
    {
        public PDFMetaDataDictionary(PDFDictionary dictionary) : base(dictionary)
        {
            
        }
    }
}