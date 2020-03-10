using FirePDF;
using System;

namespace Samples
{
    public class SplitPages
    {
        public void Main()
        {
            const string inputFile = @"";
            const string outputFolder = @"";

            using (Pdf pdf = new Pdf(inputFile))
            {
                int i = 1;
                foreach (Page page in pdf)
                {
                    Console.WriteLine($"Running for page {i} of {pdf.NumPages()}");

                    using (Pdf newPdf = new Pdf())
                    {
                        newPdf.AddPage(page);
                        newPdf.Save(outputFolder + i + ".Pdf");
                    }

                    i++;
                }
            }
        }
    }
}
