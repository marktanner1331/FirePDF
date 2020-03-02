using FirePDF;
using System;
using System.Collections.Generic;
using System.Text;

namespace Samples
{
    public class SplitPages
    {
        public void main()
        {
            string inputFile = @"";
            string outputFolder = @"";

            using (PDF pdf = new PDF(inputFile))
            {
                int i = 1;
                foreach (Page page in pdf)
                {
                    Console.WriteLine($"Running for page {i} of {pdf.numPages()}");

                    using (PDF newPDF = new PDF())
                    {
                        newPDF.addPage(page);
                        newPDF.save(outputFolder + i + ".pdf");
                    }

                    i++;
                }
            }
        }
    }
}
