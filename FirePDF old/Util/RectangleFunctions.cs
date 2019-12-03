using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FirePDF.Util
{
    public static class RectangleFunctions
    {
        public static bool isSubRect(RectangleF parent, RectangleF child)
        {
            return child.X > parent.X
                && child.Y + child.Height < parent.Y + parent.Height
                && child.Y > parent.Y
                && child.X + child.Width < parent.X + parent.Width;
        }

        /// <summary>
        /// returns intersection of all given rectangles
        /// </summary>
        public static RectangleF intersectRectangles(IEnumerable<RectangleF> rectangles)
        {
            float x = rectangles.Select(k => k.X).Max();
            float y = rectangles.Select(k => k.Y).Max();
            float width = rectangles.Select(k => k.X + k.Width).Max() - x;
            float height = rectangles.Select(k => k.Y + k.Height).Max() - y;

            return new RectangleF(x, y, width, height);
        }
    }
}
