using FirePDF;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace FirePDFTests
{
    [TestClass()]
    public class NewPDFTests
    {
        private string getPDFFolder()
        {
            return System.IO.Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) + "/../../pdfs/";
        }

        private string getOutputPDFFolder()
        {
            string folder = getPDFFolder() + "output/";
            if(Directory.Exists(folder) == false)
            {
                Directory.CreateDirectory(folder);
            }

            return folder;
        }

        [TestMethod()]
        public void createPDFTest1()
        {
            PDF pdf = new PDF();
            Page page = new Page(pdf, new System.Drawing.Size(300, 824));
            pdf.addPage(page);

            string temp = getOutputPDFFolder() + "createPDFTest1.pdf";
            if(File.Exists(temp))
            {
                File.Delete(temp);
            }

            pdf.save(temp);
        }

        [TestMethod()]
        public void createPDFTest2()
        {
            PDF pdf = new PDF(getPDFFolder() + "09a9da81-1261-49b3-a9f0-2e76b476f992.pdf");
            Page page1 = pdf.getPage(1);

            Page newPage = new Page(pdf, page1.boundingBox.Size.ToSize());
            pdf.addPage(newPage);

            string temp = getOutputPDFFolder() + "createPDFTest2.pdf";
            if (File.Exists(temp))
            {
                File.Delete(temp);
            }

            pdf.save(temp);
        }
    }
}
