using Microsoft.VisualStudio.TestTools.UnitTesting;
using FirePDF.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Reflection;
using System.Drawing;

namespace FirePDF.Model.Tests
{
    [TestClass()]
    public class XObjectImageTests
    {
        private string getPDFFolder()
        {
            return System.IO.Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) + "/../../pdfs/";
        }

        [TestMethod()]
        public void getImageTest()
        {
            string file = getPDFFolder() + "page 24 fixed.pdf";
            PDF pdf = new PDF(file);

            Page page = pdf.getPage(1);

            List<string> forms = page.resources.listXObjectForms().ToList();
            XObjectForm form = page.resources.getXObjectForm(forms.First());

            XObjectImage xObjectImage = form.resources.getXObjectImage("img0");
            Image image = xObjectImage.getImage();
        }
    }
}