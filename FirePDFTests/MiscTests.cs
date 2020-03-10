using System.Drawing.Drawing2D;
using System.IO;
using System.Reflection;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace FirePDFTests
{
    [TestClass()]
    public class MiscTests
    {
        private string GetPdfFolder()
        {
            return Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) + "/../../pdfs/";
        }

        [TestMethod()]
        public void MatrixTest()
        {
            Matrix m = new Matrix();
            const float width = 500;

            m.Multiply(new Matrix((width + 1) / width, 0, 0, 1, 0, 0));
            m.Multiply(new Matrix(width / (width + 1), 0, 0, 1, 0, 0));

            Assert.AreEqual(1, m.Elements[0]);
        }
    }
}
