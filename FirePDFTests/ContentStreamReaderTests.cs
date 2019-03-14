using Microsoft.VisualStudio.TestTools.UnitTesting;
using FirePDF;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Reflection;
using FirePDF.Reading;

namespace FirePDF.Tests
{
    [TestClass()]
    public class ContentStreamReaderTests
    {
        private string getPDFFolder()
        {
            return Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) + "/../../pdfs/";
        }

        [TestMethod()]
        public void readContentStreamTest()
        {
            string file = getPDFFolder() + "pb13332-cop-cats-091204.pdf";
            PDF pdf = new PDF(file);

            Page page = pdf.getPage(1);
            Stream s = PDFReaderLayer2.readContentStream(page);
            List<Operation> operations = ContentStreamReader.readContentStream(s);

            Assert.AreEqual(4858, operations.Count);
        }
    }
}