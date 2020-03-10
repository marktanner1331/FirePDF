using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Drawing.Drawing2D;

namespace FirePDFTests
{
    [TestClass]
    public class GraphicsPathTests
    {
        [TestMethod]
        public void CurrentPointTest()
        {
            GraphicsPath path = new GraphicsPath();

            path.AddLine(0, 0, 0, 0);
            int k = path.PointCount;
        }
    }
}
