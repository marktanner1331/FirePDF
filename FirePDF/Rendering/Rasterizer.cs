using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FirePDF.Model;

namespace FirePDF.Rendering
{
    public class Rasterizer : IRenderer
    {
        private Graphics graphics;

        public Rasterizer(Graphics graphics)
        {
            this.graphics = graphics;
        }

        public override void willStartRenderingPage(RectangleF boundingBox, Func<Model.GraphicsState> getGraphicsState)
        {
            base.willStartRenderingPage(boundingBox, getGraphicsState);

            float scale = dpi / 72f;

            Model.GraphicsState graphicsState = getGraphicsState();
            graphicsState.currentTransformationMatrix.Translate(0, scale * boundingBox.Height);
            graphicsState.currentTransformationMatrix.Scale(scale, -scale);
        }
        
        /// <summary>
        /// refreshes the state of the graphicsContext from the graphics state returned by getGraphicsState
        /// </summary>
        private void refreshGraphicsState()
        {
            Model.GraphicsState gs = getGraphicsState();

            graphics.Transform = new Matrix();
            graphics.SetClip(gs.clippingPath, CombineMode.Replace);
            
            graphics.Transform = gs.currentTransformationMatrix;
        }

        public override void drawImage(XObjectImage image)
        {
            refreshGraphicsState();

            Matrix temp = graphics.Transform.Clone();
            
            temp.Scale(1, -1);
            temp.Translate(0, -1);
            
            graphics.Transform = temp;
            graphics.DrawImage(image.getImage(), 0, 0, 1, 1);
            graphics.Transform = getGraphicsState().currentTransformationMatrix;
        }

        public override void fillAndStrokePath(GraphicsPath path)
        {
            fillPath(path);
            strokePath(path);
        }

        public override void fillPath(GraphicsPath path)
        {
            refreshGraphicsState();

            Brush b = new SolidBrush(getGraphicsState().nonStrokingColor);
            graphics.FillPath(b, path);
        }

        public override void strokePath(GraphicsPath path)
        {
            refreshGraphicsState();

            Model.GraphicsState gs = getGraphicsState();

            Pen p = new Pen(gs.strokingColor, gs.lineWidth);
            graphics.DrawPath(p, path);
        }

        public override void drawText(byte[] text)
        {
            refreshGraphicsState();

            FirePDF.Model.GraphicsState gs = getGraphicsState();
            Matrix textRenderingMatrix = new Matrix(gs.fontSize * gs.horizontalScaling, 0, 0, gs.fontSize, 0, gs.textRise);
            textRenderingMatrix.Multiply(gs.textMatrix, MatrixOrder.Append);
            textRenderingMatrix.Multiply(gs.currentTransformationMatrix, MatrixOrder.Append);

            graphics.Transform = textRenderingMatrix;

            Matrix temp = graphics.Transform.Clone();

            temp.Scale(1, -1);
            temp.Translate(0, -1);

            graphics.Transform = temp;

            string textString = gs.font.readUnicodeStringFromHexString(text);

            graphics.DrawString(textString, new System.Drawing.Font(FontFamily.GenericSerif, 1), new SolidBrush(gs.nonStrokingColor), PointF.Empty);
            graphics.Transform = getGraphicsState().currentTransformationMatrix;
        }
    }
}
