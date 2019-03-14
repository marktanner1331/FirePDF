using Microsoft.VisualStudio.TestTools.UnitTesting;
using FirePDF.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FirePDF.Model.Tests
{
    [TestClass()]
    public class RectangleTests
    {
        [TestMethod()]
        public void RectangleTest()
        {
            Rectangle rect = new Rectangle(new List<object> { 0, 0, 0, 0} );
        }

        [TestMethod()]
        public void RectangleTest2()
        {
            Rectangle rect = new Rectangle(new List<object> { 0.0, 0, 0, 0 });
        }
    }
}