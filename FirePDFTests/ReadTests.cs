using FirePDF;
using FirePDF.Model;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Reflection;

namespace FirePDFTests
{
    [TestClass]
    public class ReadTests
    {
        private static string GetPdfFolder()
        {
            return Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) + "/../../pdfs/";
        }

        private static string GetOutputPdfFolder()
        {
            string folder = GetPdfFolder() + "output/";
            if (Directory.Exists(folder) == false)
            {
                Directory.CreateDirectory(folder);
            }

            return folder;
        }

        [TestMethod]
        public void TestPdf()
        {
            string file = GetPdfFolder() + "page 24 fixed.Pdf";
            using (Pdf pdf = new Pdf(file))
            {
                pdf.InsertPage(new Page(pdf, new Size(650, 800)), 1);

                string temp = GetOutputPdfFolder() + Path.GetFileNameWithoutExtension(file) + " full_read_write.Pdf";
                if (File.Exists(temp))
                {
                    File.Delete(temp);
                }

                pdf.Save(temp);
            }
        }

        [TestMethod]
        public void TestPdfUpdate()
        {
            string file = GetPdfFolder() + "page 24 fixed.Pdf";
            using (Pdf pdf = new Pdf(file))
            {
                pdf.InsertPage(new Page(pdf, new Size(650, 800)), 1);

                string temp = GetOutputPdfFolder() + Path.GetFileNameWithoutExtension(file) + " update.Pdf";
                if (File.Exists(temp))
                {
                    File.Delete(temp);
                }

                pdf.Save(temp, SaveType.Update);
            }
        }

        [TestMethod]
        public void TestAllPdFs()
        {
            List<string> inputFiles = Directory.EnumerateFiles(GetPdfFolder()).ToList();
            foreach (string file in inputFiles)
            {
                Pdf pdf = new Pdf(file);
                Pdf newPdf = new Pdf();

                foreach (Page page in pdf)
                {
                    newPdf.AddPage(page);
                }

                string temp = GetOutputPdfFolder() + Path.GetFileNameWithoutExtension(file) + " full_read_write.Pdf";
                if (File.Exists(temp))
                {
                    File.Delete(temp);
                }

                pdf.Save(temp);
            }
        }

        [TestMethod]
        public void TestAllPdFs2()
        {
            List<string> inputFiles = Directory.EnumerateFiles(GetPdfFolder()).ToList();
            foreach (string file in inputFiles)
            {
                using (Pdf pdf = new Pdf(file))
                {
                    pdf.InsertPage(new Page(pdf, new Size(650, 800)), 1);

                    string temp = GetOutputPdfFolder() + Path.GetFileNameWithoutExtension(file) + " full_read_write.Pdf";
                    if (File.Exists(temp))
                    {
                        File.Delete(temp);
                    }

                    pdf.Save(temp);
                }
            }
        }
    }
}
