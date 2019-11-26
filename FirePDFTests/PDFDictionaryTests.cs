using FirePDF.Model;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FirePDFTests
{
    [TestClass]
    public class PDFDictionaryTests
    {
        [TestMethod]
        public void testMissingKey1()
        {
            PDFDictionary dict = new PDFDictionary(null);
            var k = dict.get<int?>("hi");
            Assert.IsNull(k);
        }

        [TestMethod]
        public void testMissingKey2()
        {
            PDFDictionary dict = new PDFDictionary(null);
            var k = dict.get<int>("hi");
            Assert.AreEqual(0, k);
        }
    }
}
