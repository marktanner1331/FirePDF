using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FirePDF.Model
{
    public class GraphicsState
    {
        public Matrix currentTransformationMatrix { get; private set; }
        public float flatnessTolerance;

        public Color nonStrokingColor;
        public Color strokingColor;

        public GraphicsState()
        {
            currentTransformationMatrix = new Matrix();
            flatnessTolerance = 0;
            nonStrokingColor = Color.Black;
            strokingColor = Color.Black;
        }

        public GraphicsState clone()
        {
            return new GraphicsState
            {
                currentTransformationMatrix = currentTransformationMatrix.Clone(),
                flatnessTolerance = flatnessTolerance,
                nonStrokingColor = nonStrokingColor,
                strokingColor = strokingColor
            };
        }

        public void intersectClippingPath(GraphicsPath currentPath)
        {
            
        }
    }
}
