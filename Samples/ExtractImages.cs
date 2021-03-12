using FirePDF;
using FirePDF.Model;
using System;
using System.Collections.Generic;
using System.Text;

namespace Samples
{
    class ExtractImages
    {
        internal static void Main(string[] args)
        {
            const string file = @"C:\Users\Mark Tanner\scratch\city.pdf";
            using (Pdf pdf = new Pdf(file))
            {
                Page page = pdf.GetPage(1);
                var k = page.ListImages();
                foreach (var g in k)
                {
                    var q = g.Get<XObjectImage>();
                    q.GetImage().Save(@"C:\Users\Mark Tanner\scratch\" + g.objectNumber + ".jpg");
                }
            }
        }
    }
}
