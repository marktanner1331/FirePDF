using Microsoft.VisualStudio.TestTools.UnitTesting;
using FirePDF;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Reflection;
using System.IO;
using System.Drawing;
using FirePDF.Reading;

namespace FirePDF.Tests
{
    [TestClass()]
    public class PageTests
    {
        private string getPDFFolder()
        {
            return Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) + "/../../pdfs/";
        }

        [TestMethod()]
        public void getImagesTest()
        {
            string file = getPDFFolder() + "pb13332-cop-cats-091204.pdf";
            PDF pdf = new PDF(file);

            Page page = pdf.getPage(1);
            Assert.AreEqual(1, page.getImages().Count());
        }

        [TestMethod()]
        public void readFormTest()
        {
            string file = getPDFFolder() + "page 24 fixed.pdf";
            PDF pdf = new PDF(file);

            Page page = pdf.getPage(1);

            List<XObjectForm> forms = page.getXObjectForms().ToList();
            XObjectForm form = forms.First();

            Stream s = PDFReaderLayer2.readContentStream(form);
            List<Operation> operations = ContentStreamReader.readContentStream(s);

            Assert.AreEqual(600, operations.Count);
        }

        [TestMethod()]
        public void getContentStreamTest()
        {
            string file = getPDFFolder() + "pb13332-cop-cats-091204.pdf";
            PDF pdf = new PDF(file);

            Page page = pdf.getPage(1);
            Stream s = PDFReaderLayer2.readContentStream(page);

            using (System.IO.StreamReader streamReader = new System.IO.StreamReader(s))
            {
                string str = streamReader.ReadToEnd();
            }
        }
    }
}