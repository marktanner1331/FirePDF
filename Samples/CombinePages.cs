using FirePDF;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace Samples
{
    public class CombinePages
    {
        public void main()
        {
            string inputFolder = @"";
            string outputFile = @"f";

            using (PDF newPDF = new PDF())
            {
                foreach (string file in Directory.GetFiles(inputFolder, "*.pdf"))
                {
                    using (PDF inputPDF = new PDF(file))
                    {
                        foreach (Page page in inputPDF)
                        {
                            newPDF.addPage(page);
                        }
                    }
                }

                if (File.Exists(outputFile))
                {
                    File.Delete(outputFile);
                }

                newPDF.save(outputFile);
            }
        }
    }
}
