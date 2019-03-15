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

            List<XObjectForm> forms = page.getXObjectForms().ToList();
            XObjectForm form = forms.First();

            Stream s = PDFReaderLayer2.readContentStream(form);
            List<Operation> operations = ContentStreamReader.readContentStream(s);

            GraphicsStateProcessor gsp = new GraphicsStateProcessor(form, form.getBoundingBox());
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

            List<XObjectForm> forms = page.getXObjectForms().ToList();
            XObjectForm form = forms.First();

            Stream s = PDFReaderLayer2.readContentStream(form);
            List<Operation> operations = ContentStreamReader.readContentStream(s);

            GraphicsStateProcessor gsp = new GraphicsStateProcessor(form, form.getBoundingBox());

            StringBuilder sb = new StringBuilder();
            LoggingRenderer renderer = new LoggingRenderer(gsp.getCurrentState, form, x => sb.AppendLine(x));

            LineProcessor lp = new LineProcessor(gsp.getCurrentState, renderer);

            foreach (Operation operation in operations)
            {
                gsp.processOperation(operation);
                lp.processOperation(operation);
            }
        }
    }
}
