using FirePDF;
using FirePDF.Processors;
using FirePDF.Reading;
using FirePDF.Rendering;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Flattener
{
    class Program
    {
        static void Main(string[] args)
        {
            string file = @"C:\Users\Mark Tanner\scratch\sarie page 1.pdf";
            PDF pdf = new PDF(file);

            Page page = pdf.getPage(1);
            
            IRenderer renderer = new Splitter();

            StreamProcessor sp = new StreamProcessor(renderer);
            RecursiveStreamReader streamReader = new RecursiveStreamReader(sp);

            streamReader.readStreamRecursively(page);
        }
    }
}
