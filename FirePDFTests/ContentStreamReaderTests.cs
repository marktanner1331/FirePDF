using System.Collections.Generic;
using System.IO;
using System.Reflection;
using FirePDF;
using FirePDF.Model;
using FirePDF.Reading;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace FirePDFTests
{
    [TestClass()]
    public class ContentStreamReaderTests
    {
        private static string GetPdfFolder()
        {
            return Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) + "/../../pdfs/";
        }

        [TestMethod()]
        public void ReadContentStreamTest()
        {
            string file = GetPdfFolder() + "pb13332-cop-cats-091204.Pdf";
            Pdf pdf = new Pdf(file);

            Page page = pdf.GetPage(1);
            Stream s = page.GetStream();
            List<Operation> operations = ContentStreamReader.ReadOperationsFromStream(pdf, s);

            Assert.AreEqual(4858, operations.Count);
        }
    }
}