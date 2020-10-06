using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using FirePDF;
using FirePDF.Model;
using FirePDF.Reading;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace FirePDFTests
{
    [TestClass()]
    public class PageTests
    {
        private static string GetPdfFolder()
        {
            return Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) + "/../../pdfs/";
        }

        [TestMethod()]
        public void GetImagesTest()
        {
            string file = GetPdfFolder() + "pb13332-cop-cats-091204.Pdf";
            Pdf pdf = new Pdf(file);

            Page page = pdf.GetPage(1);
            Assert.AreEqual(1, page.Resources.ListXObjectImageNames().Count());
        }

        [TestMethod()]
        public void ReadFormTest()
        {
            string file = GetPdfFolder() + "page 24 fixed.Pdf";
            Pdf pdf = new Pdf(file);

            Page page = pdf.GetPage(1);

            List<Name> forms = page.Resources.ListFormXObjectNames().ToList();
            XObjectForm form = page.Resources.GetXObjectForm(forms.First());

            Stream s = form.GetStream();
            List<Operation> operations = ContentStreamReader.ReadOperationsFromStream(pdf, s);

            Assert.AreEqual(601, operations.Count);
        }

        [TestMethod()]
        public void GetContentStreamTest()
        {
            string file = GetPdfFolder() + "pb13332-cop-cats-091204.Pdf";
            Pdf pdf = new Pdf(file);

            Page page = pdf.GetPage(1);
            Stream s = page.GetStream();

            using (StreamReader streamReader = new StreamReader(s))
            {
                string str = streamReader.ReadToEnd();
            }
        }
    }
}