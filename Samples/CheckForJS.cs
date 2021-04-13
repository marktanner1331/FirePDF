using FirePDF;
using FirePDF.Model;
using System;
using System.Collections.Generic;
using System.Text;

namespace Samples
{
    class CheckForJS
    {
        internal static void Main(string[] args)
        {
            const string file = @"C:\Users\Mark\Documents\pagesuite\tickets\CS-791 - MTM\df9322dd-a3dc-4e39-ba2b-152b24b2a029.pdf";
            using (Pdf pdf = new Pdf(file))
            {
                foreach(PdfDictionary dict in pdf.GetAll<PdfDictionary>())
                {
                    if(dict.ContainsKey("JS"))
                    {
                        Console.WriteLine("PDF contains JS!");
                        break;
                    }
                }
            }
        }
    }
}
