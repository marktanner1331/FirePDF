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
    }
}
