using System;
using System.Collections.Generic;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FirePDF.Model
{
    public class GraphicsState
    {
        public Matrix currentTransformationMatrix { get; private set; }

        public GraphicsState()
        {
            currentTransformationMatrix = new Matrix();
        }

        public GraphicsState clone()
        {
            return new GraphicsState
            {
                currentTransformationMatrix = currentTransformationMatrix.Clone()
            };
        }

        public void intersectClippingPath(GraphicsPath currentPath)
        {
            
        }
    }
}
