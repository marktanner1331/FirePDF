using FirePDF;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.IO;
using System.Reflection;

namespace FirePDFTests
{
    [TestClass()]
    public class NewPdfTests
    {
        private static string GetPdfFolder()
        {
            return Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) + "/../../pdfs/";
        }

        private static string GetOutputPdfFolder()
        {
            string folder = GetPdfFolder() + "output/";
            if(Directory.Exists(folder) == false)
            {
                Directory.CreateDirectory(folder);
            }

            return folder;
        }

        [TestMethod()]
        public void CreatePdfTest1()
        {
            Pdf pdf = new Pdf();
            Page page = new Page(pdf, new System.Drawing.Size(300, 824));
            pdf.AddPage(page);

            string temp = GetOutputPdfFolder() + "createPDFTest1.Pdf";
            if(File.Exists(temp))
            {
                File.Delete(temp);
            }

            pdf.Save(temp);
        }

        [TestMethod()]
        public void CreatePdfTest2()
        {
            Pdf pdf = new Pdf(GetPdfFolder() + "09a9da81-1261-49b3-a9f0-2e76b476f992.Pdf");
            Page page1 = pdf.GetPage(1);

            Page newPage = new Page(pdf, page1.BoundingBox.Size.ToSize());
            pdf.AddPage(newPage);

            string temp = GetOutputPdfFolder() + "createPDFTest2.Pdf";
            if (File.Exists(temp))
            {
                File.Delete(temp);
            }

            pdf.Save(temp);
        }
    }
}
