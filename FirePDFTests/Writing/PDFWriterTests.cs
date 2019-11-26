using Microsoft.VisualStudio.TestTools.UnitTesting;
using FirePDF.Writing;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Reflection;
using FirePDF.Model;

namespace FirePDF.Writing.Tests
{
    [TestClass()]
    public class PDFWriterTests
    {
        private string getPDFFolder()
        {
            return System.IO.Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) + "/../../pdfs/";
        }

        [TestMethod()]
        public void writeHeaderTest()
        {
            using (MemoryStream ms = new MemoryStream())
            {
                using (PDFWriter writer = new PDFWriter(ms, true))
                {
                    writer.writeHeader(1.4);
                }

                ms.Position = 0;
                string contents = Encoding.ASCII.GetString(ms.ToArray());
                Assert.AreEqual("%PDF-1.4\n%????\r\n", contents);
            }
        }

        [TestMethod()]
        public void writeResourcesTest()
        {
            string file = getPDFFolder() + "page 24 fixed.pdf";
            PDF pdf = new PDF(file);
            Page page = pdf.getPage(1);

            using (MemoryStream ms = new MemoryStream())
            {
                using (PDFWriter writer = new PDFWriter(ms, true))
                {
                    string formName = page.resources.listXObjectForms().First();
                    XObjectForm form = page.resources.getXObjectForm(formName);

                    page.resources.overwriteXObject(form, formName);
                    writer.writeUpdatedPDF(pdf);
                }
                
                ms.Position = 0;
                string contents = Encoding.ASCII.GetString(ms.ToArray());
            }
        }
    }
}