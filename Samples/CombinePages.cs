using FirePDF;
using System.IO;

namespace Samples
{
    public class CombinePages
    {
        public void Main()
        {
            const string inputFolder = @"";
            const string outputFile = @"f";

            using (Pdf newPdf = new Pdf())
            {
                foreach (string file in Directory.GetFiles(inputFolder, "*.Pdf"))
                {
                    using (Pdf inputPdf = new Pdf(file))
                    {
                        foreach (Page page in inputPdf)
                        {
                            newPdf.AddPage(page);
                        }
                    }
                }

                if (File.Exists(outputFile))
                {
                    File.Delete(outputFile);
                }

                newPdf.Save(outputFile);
            }
        }
    }
}
