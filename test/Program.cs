using FirePDF;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace test
{
    class Program
    {
        static void Main(string[] args)
        {
            string file = @"C:\Users\Work\Documents\c#\FirePDF\pdfs\pb13332-cop-cats-091204.pdf";
            PDF pdf = new PDF(file);
        }
    }
}
