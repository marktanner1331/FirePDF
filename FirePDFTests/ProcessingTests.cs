using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using FirePDF;
using FirePDF.Model;
using FirePDF.Processors;
using FirePDF.Reading;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace FirePDFTests
{
    [TestClass()]
    public class ProcessingTests
    {
        private static string GetPdfFolder()
        {
            return Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) + "/../../pdfs/";
        }

        [TestMethod()]
        public void GraphicsProcessorTest()
        {
            string file = GetPdfFolder() + "page 24 fixed.Pdf";
            Pdf pdf = new Pdf(file);

            Page page = pdf.GetPage(1);

            List<Name> forms = page.Resources.ListXObjectForms().ToList();
            XObjectForm form = page.Resources.GetXObjectForm(forms.First());

            Stream s = form.GetStream();
            List<Operation> operations = ContentStreamReader.ReadOperationsFromStream(pdf, s);

            GraphicsStateProcessor gsp = new GraphicsStateProcessor(() => form.Resources, form.BoundingBox);
            foreach (Operation operation in operations)
            {
                gsp.ProcessOperation(operation);
            }
        }

        [TestMethod()]
        public void LineProcessorTest()
        {
            string file = GetPdfFolder() + "page 24 fixed.Pdf";
            Pdf pdf = new Pdf(file);

            Page page = pdf.GetPage(1);

            List<Name> forms = page.Resources.ListXObjectForms().ToList();
            XObjectForm form = page.Resources.GetXObjectForm(forms.First());

            Stream s = form.GetStream();
            List<Operation> operations = ContentStreamReader.ReadOperationsFromStream(pdf, s);
        }

        [TestMethod()]
        public void ImageProcessorTest()
        {
            string file = GetPdfFolder() + "page 24 fixed.Pdf";
            Pdf pdf = new Pdf(file);

            Page page = pdf.GetPage(1);

            List<Name> forms = page.Resources.ListXObjectForms().ToList();
            XObjectForm form = page.Resources.GetXObjectForm(forms.First());

            Stream s = form.GetStream();
        }

        [TestMethod()]
        public void ImageProcessorRenderingTest()
        {
            string file = GetPdfFolder() + "page 24 fixed.Pdf";
            Pdf pdf = new Pdf(file);

            Page page = pdf.GetPage(1);

            List<Name> forms = page.Resources.ListXObjectForms().ToList();
            XObjectForm form = page.Resources.GetXObjectForm(forms.First());

            Stream s = form.GetStream();
            //List<Operation> operations = ContentStreamReader.readOperationsFromStream(s);

            //GraphicsStateProcessor gsp = new GraphicsStateProcessor(form);

            //RectangleF bounds = form.boundingBox;
            //Bitmap image = new Bitmap((int)bounds.Width, (int)bounds.Height);
            //Graphics g = Graphics.FromImage(image);
            
            //Rasterizer renderer = new Rasterizer(g);
            //StreamProcessor sp = new StreamProcessor(renderer);
            //sp.render(form, operations);
        }
    }
}
