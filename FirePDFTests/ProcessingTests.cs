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

            List<XObjectForm> forms = page.resources.getXObjectForms().ToList();
            XObjectForm form = forms.First();

            Stream s = PDFReaderLayer2.readContentStream(form);
            List<Operation> operations = ContentStreamReader.readContentStream(s);

            GraphicsStateProcessor gsp = new GraphicsStateProcessor(form);
            foreach(Operation operation in operations)
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

            List<XObjectForm> forms = page.resources.getXObjectForms().ToList();
            XObjectForm form = forms.First();

            Stream s = PDFReaderLayer2.readContentStream(form);
            List<Operation> operations = ContentStreamReader.readContentStream(s);

            GraphicsStateProcessor gsp = new GraphicsStateProcessor(form);

            StringBuilder sb = new StringBuilder();
            LoggingRenderer renderer = new LoggingRenderer(gsp.getCurrentState, form, x => sb.AppendLine(x));

            LineProcessor lp = new LineProcessor(gsp.getCurrentState, renderer);

            foreach (Operation operation in operations)
            {
                gsp.processOperation(operation);
                lp.processOperation(operation);
            }
        }

        [TestMethod()]
        public void imageProcessorTest()
        {
            string file = getPDFFolder() + "page 24 fixed.pdf";
            PDF pdf = new PDF(file);

            Page page = pdf.getPage(1);

            List<XObjectForm> forms = page.resources.getXObjectForms().ToList();
            XObjectForm form = forms.First();

            Stream s = PDFReaderLayer2.readContentStream(form);
            List<Operation> operations = ContentStreamReader.readContentStream(s);

            GraphicsStateProcessor gsp = new GraphicsStateProcessor(form);

            StringBuilder sb = new StringBuilder();
            LoggingRenderer renderer = new LoggingRenderer(gsp.getCurrentState, form, x => sb.AppendLine(x));

            LineProcessor lp = new LineProcessor(gsp.getCurrentState, renderer);
            ImageProcessor ip = new ImageProcessor(form, renderer);

            foreach (Operation operation in operations)
            {
                gsp.processOperation(operation);
                lp.processOperation(operation);
                ip.processOperation(operation);
            }
        }

        [TestMethod()]
        public void imageProcessorRenderingTest()
        {
            string file = getPDFFolder() + "page 24 fixed.pdf";
            PDF pdf = new PDF(file);

            Page page = pdf.getPage(1);

            List<XObjectForm> forms = page.resources.getXObjectForms().ToList();
            XObjectForm form = forms.First();

            Stream s = PDFReaderLayer2.readContentStream(form);
            List<Operation> operations = ContentStreamReader.readContentStream(s);

            GraphicsStateProcessor gsp = new GraphicsStateProcessor(form);

            FirePDF.Model.Rectangle bounds = form.boundingBox;
            Bitmap image = new Bitmap((int)bounds.width, (int)bounds.height);
            Graphics g = Graphics.FromImage(image);

            RectangleF rect = new RectangleF
            {
                X = bounds.left,
                Y = bounds.bottom,
                Width = bounds.width,
                Height = bounds.height
            };

            Renderer renderer = new Renderer(g, rect, gsp.getCurrentState, form);
            LineProcessor lp = new LineProcessor(gsp.getCurrentState, renderer);
            ImageProcessor ip = new ImageProcessor(form, renderer);

            foreach (Operation operation in operations)
            {
                gsp.processOperation(operation);
                lp.processOperation(operation);
                ip.processOperation(operation);
            }
        }
    }
}
