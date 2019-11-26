using FirePDF;
using FirePDF.Model;
using FirePDF.Reading;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Reflection;
using FirePDF.Processors;
using FirePDF.Rendering;
using System.Drawing;

namespace FirePDF.Tests
{
    [TestClass()]
    public class ProcessingTests
    {
        private string getPDFFolder()
        {
            return System.IO.Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) + "/../../pdfs/";
        }

        [TestMethod()]
        public void graphicsProcessorTest()
        {
            string file = getPDFFolder() + "page 24 fixed.pdf";
            PDF pdf = new PDF(file);

            Page page = pdf.getPage(1);

            List<Name> forms = page.resources.listXObjectForms().ToList();
            XObjectForm form = page.resources.getXObjectForm(forms.First());

            Stream s = form.getStream();
            List<Operation> operations = ContentStreamReader.readOperationsFromStream(pdf, s);

            GraphicsStateProcessor gsp = new GraphicsStateProcessor(() => form.resources, form.boundingBox);
            foreach (Operation operation in operations)
            {
                gsp.processOperation(operation);
            }
        }

        [TestMethod()]
        public void lineProcessorTest()
        {
            string file = getPDFFolder() + "page 24 fixed.pdf";
            PDF pdf = new PDF(file);

            Page page = pdf.getPage(1);

            List<Name> forms = page.resources.listXObjectForms().ToList();
            XObjectForm form = page.resources.getXObjectForm(forms.First());

            Stream s = form.getStream();
            List<Operation> operations = ContentStreamReader.readOperationsFromStream(pdf, s);
        }

        [TestMethod()]
        public void imageProcessorTest()
        {
            string file = getPDFFolder() + "page 24 fixed.pdf";
            PDF pdf = new PDF(file);

            Page page = pdf.getPage(1);

            List<Name> forms = page.resources.listXObjectForms().ToList();
            XObjectForm form = page.resources.getXObjectForm(forms.First());

            Stream s = form.getStream();
        }

        [TestMethod()]
        public void imageProcessorRenderingTest()
        {
            string file = getPDFFolder() + "page 24 fixed.pdf";
            PDF pdf = new PDF(file);

            Page page = pdf.getPage(1);

            List<Name> forms = page.resources.listXObjectForms().ToList();
            XObjectForm form = page.resources.getXObjectForm(forms.First());

            Stream s = form.getStream();
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
