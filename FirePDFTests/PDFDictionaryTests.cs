using FirePDF.Model;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace FirePDFTests
{
    [TestClass]
    public class PdfDictionaryTests
    {
        [TestMethod]
        public void TestMissingKey1()
        {
            PdfDictionary dict = new PdfDictionary(null);
            int? k = dict.Get<int?>("hi");
            Assert.IsNull(k);
        }

        [TestMethod]
        public void TestMissingKey2()
        {
            PdfDictionary dict = new PdfDictionary(null);
            int k = dict.Get<int>("hi");
            Assert.AreEqual(0, k);
        }
    }
}
