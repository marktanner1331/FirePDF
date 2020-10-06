using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using FirePDF;
using FirePDF.Model;
using FirePDF.Writing;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace FirePDFTests.Writing
{
    [TestClass()]
    public class PdfWriterTests
    {
        private static string GetPdfFolder()
        {
            return Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) + "/../../pdfs/";
        }

        [TestMethod()]
        public void WriteHeaderTest()
        {
            using (MemoryStream ms = new MemoryStream())
            {
                using (PdfWriter writer = new PdfWriter(ms, true))
                {
                    writer.WriteHeader(1.4);
                }

                ms.Position = 0;
                string contents = Encoding.ASCII.GetString(ms.ToArray());
                Assert.AreEqual("%Pdf-1.4\n%????\r\n", contents);
            }
        }

        [TestMethod()]
        public void WriteResourcesTest()
        {
            string file = GetPdfFolder() + "page 24 fixed.Pdf";
            Pdf pdf = new Pdf(file);
            Page page = pdf.GetPage(1);

            using (MemoryStream ms = new MemoryStream())
            {
                using (PdfWriter writer = new PdfWriter(ms, true))
                {
                    string formName = page.Resources.ListFormXObjectNames().First();
                    XObjectForm form = page.Resources.GetXObjectForm(formName);

                    page.Resources.OverwriteXObject(form, formName);
                    writer.WriteUpdatedPdf(pdf);
                }
                
                ms.Position = 0;
                string contents = Encoding.ASCII.GetString(ms.ToArray());
            }
        }
    }
}