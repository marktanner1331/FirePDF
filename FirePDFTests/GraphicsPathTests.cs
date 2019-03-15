using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FirePDFTests
{
    [TestClass]
    public class GraphicsPathTests
    {
        [TestMethod]
        public void currentPointTest()
        {
            GraphicsPath path = new GraphicsPath();

            path.AddLine(0, 0, 0, 0);
            var k = path.PointCount;
        }
    }
}
