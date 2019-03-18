using FirePDF;
using FirePDF.Model;
using FirePDF.Processors;
using FirePDF.Reading;
using FirePDF.Rendering;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace test
{
    class Program
    {
        static void Main(string[] args)
        {
            string file = @"C:\Users\Mark Tanner\scratch\page 24 fixed.pdf";
            PDF pdf = new PDF(file);

            Page page = pdf.getPage(1);

            List<XObjectForm> forms = page.resources.getXObjectForms().ToList();
            XObjectForm form = forms.First();

            Stream s = form.readContentStream();
            List<Operation> operations = ContentStreamReader.readOperationsFromStream(s);

            GraphicsStateProcessor gsp = new GraphicsStateProcessor(form);

            RectangleF bounds = form.boundingBox;
            Bitmap image = new Bitmap((int)bounds.Width, (int)bounds.Height);
            Graphics g = Graphics.FromImage(image);
            
            Renderer renderer = new Renderer(g, gsp.getCurrentState, form);

            LineProcessor lp = new LineProcessor(gsp.getCurrentState, renderer);

            foreach (Operation operation in operations)
            {
                gsp.processOperation(operation);
                lp.processOperation(operation);
            }
            
            image.Save(@"C:\Users\Mark Tanner\scratch\page 24 fixed.jpg");
        }
    }
}
