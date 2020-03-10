using System.IO;
using System.Reflection;
using FirePDF;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace FirePDFTests
{
    [TestClass()]
    public class PdfTests
    {
        private static string GetPdfFolder()
        {
            return Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) + "/../../pdfs/";
        }
        
        [TestMethod()]
        public void PdfTest()
        {
            string file = GetPdfFolder() + "pb13332-cop-cats-091204.Pdf";
            Pdf pdf = new Pdf(file);
        }

        [TestMethod()]
        public void NumPagesTest()
        {
            string file = GetPdfFolder() + "pb13332-cop-cats-091204.Pdf";
            Pdf pdf = new Pdf(file);

            Assert.AreEqual(19, pdf.NumPages());
        }

        [TestMethod()]
        public void GetPageTest()
        {
            string file = GetPdfFolder() + "pb13332-cop-cats-091204.Pdf";
            Pdf pdf = new Pdf(file);

            Page page = pdf.GetPage(1);
            Assert.IsNotNull(page);
        }

        [TestMethod()]
        public void GetPageTest2()
        {
            string file = GetPdfFolder() + "pb13332-cop-cats-091204.Pdf";
            Pdf pdf = new Pdf(file);

            Page page = pdf.GetPage(19);
            Assert.IsNotNull(page);
        }

        [TestMethod()]
        public void GetPageTest3()
        {
            string file = GetPdfFolder() + "pb13332-cop-cats-091204.Pdf";
            Pdf pdf = new Pdf(file);

            try
            {
                Page page = pdf.GetPage(20);
                Assert.Fail();
            }
            catch
            {

            }
        }
    }
}