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
            string file = @"C:\Users\Mark Tanner\scratch\sarie page 1.pdf";
            PDF pdf = new PDF(file);
            
            Page page = pdf.getPage(1);

            Bitmap image = new Bitmap((int)page.boundingBox.Width, (int)page.boundingBox.Height);
            Graphics graphics = Graphics.FromImage(image);

            //IRenderer renderer = new Rasterizer(graphics);
            IRenderer renderer = new LoggingRenderer(x => Debug.WriteLine(x));

            StreamProcessor sp = new StreamProcessor(renderer);
            RecursiveStreamReader streamReader = new RecursiveStreamReader(sp);

            streamReader.readPage(page);
            
            image.Save(@"C:\Users\Mark Tanner\scratch\sarie page 1.jpg");
        }
    }
}
