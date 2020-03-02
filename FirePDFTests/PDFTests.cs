using Microsoft.VisualStudio.TestTools.UnitTesting;
using FirePDF;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Reflection;
using System.IO;

namespace FirePDF.Tests
{
    [TestClass()]
    public class PDFTests
    {
        private string getPDFFolder()
        {
            return Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) + "/../../pdfs/";
        }
        
        [TestMethod()]
        public void PDFTest()
        {
            string file = getPDFFolder() + "pb13332-cop-cats-091204.pdf";
            PDF pdf = new PDF(file);
        }

        [TestMethod()]
        public void numPagesTest()
        {
            string file = getPDFFolder() + "pb13332-cop-cats-091204.pdf";
            PDF pdf = new PDF(file);

            Assert.AreEqual(19, pdf.numPages());
        }

        [TestMethod()]
        public void getPageTest()
        {
            string file = getPDFFolder() + "pb13332-cop-cats-091204.pdf";
            PDF pdf = new PDF(file);

            Page page = pdf.getPage(1);
            Assert.IsNotNull(page);
        }

        [TestMethod()]
        public void getPageTest2()
        {
            string file = getPDFFolder() + "pb13332-cop-cats-091204.pdf";
            PDF pdf = new PDF(file);

            Page page = pdf.getPage(19);
            Assert.IsNotNull(page);
        }

        [TestMethod()]
        public void getPageTest3()
        {
            string file = getPDFFolder() + "pb13332-cop-cats-091204.pdf";
            PDF pdf = new PDF(file);

            try
            {
                Page page = pdf.getPage(20);
                Assert.Fail();
            }
            catch
            {

            }
        }
    }
}