using FirePDF;
using FirePDF.Model;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace FirePDFTests
{
    [TestClass]
    public class ReadTests
    {
        private string getPDFFolder()
        {
            return System.IO.Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) + "/../../pdfs/";
        }

        private string getOutputPDFFolder()
        {
            string folder = getPDFFolder() + "output/";
            if (Directory.Exists(folder) == false)
            {
                Directory.CreateDirectory(folder);
            }

            return folder;
        }

        [TestMethod]
        public void testPDF()
        {
            string file = getPDFFolder() + "page 24 fixed.pdf";
            using (PDF pdf = new PDF(file))
            {
                pdf.insertPage(new Page(pdf, new Size(650, 800)), 1);

                string temp = getOutputPDFFolder() + Path.GetFileNameWithoutExtension(file) + " full_read_write.pdf";
                if (File.Exists(temp))
                {
                    File.Delete(temp);
                }

                pdf.save(temp);
            }
        }

        [TestMethod]
        public void testPDFUpdate()
        {
            string file = getPDFFolder() + "page 24 fixed.pdf";
            using (PDF pdf = new PDF(file))
            {
                pdf.insertPage(new Page(pdf, new Size(650, 800)), 1);

                string temp = getOutputPDFFolder() + Path.GetFileNameWithoutExtension(file) + " update.pdf";
                if (File.Exists(temp))
                {
                    File.Delete(temp);
                }

                pdf.save(temp, SaveType.Update);
            }
        }

        [TestMethod]
        public void testAllPDFs()
        {
            List<string> inputFiles = Directory.EnumerateFiles(getPDFFolder()).ToList();
            foreach (string file in inputFiles)
            {
                PDF pdf = new PDF(file);
                PDF newPDF = new PDF();

                foreach (Page page in pdf)
                {
                    newPDF.addPage(page);
                }

                string temp = getOutputPDFFolder() + Path.GetFileNameWithoutExtension(file) + " full_read_write.pdf";
                if (File.Exists(temp))
                {
                    File.Delete(temp);
                }

                pdf.save(temp);
            }
        }

        [TestMethod]
        public void testAllPDFs2()
        {
            List<string> inputFiles = Directory.EnumerateFiles(getPDFFolder()).ToList();
            foreach (string file in inputFiles)
            {
                using (PDF pdf = new PDF(file))
                {
                    pdf.insertPage(new Page(pdf, new Size(650, 800)), 1);

                    string temp = getOutputPDFFolder() + Path.GetFileNameWithoutExtension(file) + " full_read_write.pdf";
                    if (File.Exists(temp))
                    {
                        File.Delete(temp);
                    }

                    pdf.save(temp);
                }
            }
        }
    }
}
