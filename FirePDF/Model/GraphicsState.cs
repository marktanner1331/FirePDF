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

        public Matrix textMatrix;
        public Matrix textLineMatrix;
        public float lineWidth;
        public float textLeading;
        public float characterSpacing;
        public float wordSpacing;
        public float horizontalScaling;
        public float fontSize;
        public TextRenderingMode textRenderingMode;
        public float textRise;

        public float flatnessTolerance;

        public Color nonStrokingColor;
        public Color strokingColor;

        public Region clippingPath;
        public Font font;

        /// <summary>
        /// used internally for cloning
        /// </summary>
        private GraphicsState() { }

        public GraphicsState(GraphicsPath initialClippingPath)
        {
            this.clippingPath = new Region(initialClippingPath);
            
            currentTransformationMatrix = new Matrix();

            textMatrix = new Matrix();
            textLineMatrix = new Matrix();
            textLeading = 0;
            characterSpacing = 0;
            wordSpacing = 0;
            horizontalScaling = 1;
            fontSize = 0;
            textRenderingMode = TextRenderingMode.FillText;
            textRise = 0;

            flatnessTolerance = 0;
            nonStrokingColor = Color.Black;
            strokingColor = Color.Black;
            lineWidth = 1;

            font = null;
        }

        public GraphicsState clone()
        {
            return new GraphicsState
            {
                currentTransformationMatrix = currentTransformationMatrix.Clone(),
                textMatrix = textMatrix.Clone(),
                textLineMatrix = textLineMatrix.Clone(),
                textLeading = textLeading,
                characterSpacing = characterSpacing,
                wordSpacing = wordSpacing,
                horizontalScaling = horizontalScaling,
                fontSize = fontSize,
                textRenderingMode = textRenderingMode,
                textRise = textRise,
                flatnessTolerance = flatnessTolerance,
                nonStrokingColor = nonStrokingColor,
                strokingColor = strokingColor,
                clippingPath = clippingPath.Clone(),
                lineWidth = lineWidth,
                font = font,
            };
        }

        public void intersectClippingPath(GraphicsPath currentPath)
        {
            currentPath.Transform(currentTransformationMatrix);
            clippingPath.Intersect(currentPath);
        }
    }
}
