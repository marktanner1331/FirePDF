using Microsoft.VisualStudio.TestTools.UnitTesting;
using FirePDF;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Reflection;

namespace FirePDF.Tests
{
    [TestClass()]
    public class XREFStreamTests
    {
        private string getPDFFolder()
        {
            return Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) + "/../../pdfs/";
        }

        [TestMethod()]
        public void test1()
        {
            string file = getPDFFolder() + "09a9da81-1261-49b3-a9f0-2e76b476f992.pdf";
            PDF pdf = new PDF(file);

        }
    }
}