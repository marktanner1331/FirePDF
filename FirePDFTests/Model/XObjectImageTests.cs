using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Reflection;
using FirePDF;
using FirePDF.Model;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace FirePDFTests.Model
{
    [TestClass()]
    public class XObjectImageTests
    {
        private static string GetPdfFolder()
        {
            return System.IO.Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) + "/../../pdfs/";
        }

        [TestMethod()]
        public void GetImageTest()
        {
            string file = GetPdfFolder() + "page 24 fixed.Pdf";
            Pdf pdf = new Pdf(file);

            Page page = pdf.GetPage(1);

            List<Name> forms = page.Resources.ListFormXObjectNames().ToList();
            XObjectForm form = page.Resources.GetXObjectForm(forms.First());

            XObjectImage xObjectImage = form.Resources.GetXObjectImage("img0");
            Image image = xObjectImage.GetImage();
        }
    }
}