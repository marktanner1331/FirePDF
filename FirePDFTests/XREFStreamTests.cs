using System.IO;
using System.Reflection;
using FirePDF;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace FirePDFTests
{
    [TestClass()]
    public class XrefStreamTests
    {
        private static string GetPdfFolder()
        {
            return Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) + "/../../pdfs/";
        }

        [TestMethod()]
        public void Test1()
        {
            string file = GetPdfFolder() + "09a9da81-1261-49b3-a9f0-2e76b476f992.Pdf";
            Pdf pdf = new Pdf(file);

        }
    }
}