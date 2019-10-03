using FirePDF;
using FirePDF.Distilling;
using FirePDF.Model;
using FirePDF.Processors;
using FirePDF.Reading;
using FirePDF.Rendering;
using FirePDF.StreamPartFunctions;
using FirePDF.Util;
using FirePDF.Writing;
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
            string file = @"C:\Users\Mark Tanner\scratch\page 6.pdf";
            PDF pdf = new PDF(file);
            
            Page page = pdf.getPage(1);

            Bitmap image = new Bitmap((int)page.boundingBox.Width, (int)page.boundingBox.Height);
            Graphics graphics = Graphics.FromImage(image);

            XObjectForm form = (XObjectForm) page.resources.getObjectAtPath("XObject", "Fm0");

            Stream s = form.readContentStream();
            List<Operation> operations = ContentStreamReader.readOperationsFromStream(s);

            Rasterizer renderer = new Rasterizer(graphics);
            StreamProcessor sp = new StreamProcessor(renderer);
            sp.render(form, operations);

            image.Save(@"C:\Users\Mark Tanner\scratch\page 6 firepdf.jpg");
        }
    }
}
