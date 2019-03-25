using FirePDF;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Drawing.Drawing2D;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace FirePDF.Tests
{
    [TestClass()]
    public class MiscTests
    {
        private string getPDFFolder()
        {
            return Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) + "/../../pdfs/";
        }

        [TestMethod()]
        public void matrixTest()
        {
            Matrix m = new Matrix();
            float width = 500;

            m.Multiply(new Matrix((width + 1) / width, 0, 0, 1, 0, 0));
            m.Multiply(new Matrix(width / (width + 1), 0, 0, 1, 0, 0));

            Assert.AreEqual(1, m.Elements[0]);
        }
    }
}
