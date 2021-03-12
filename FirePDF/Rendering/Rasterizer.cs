using System;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Drawing2D;
using FirePDF.Model;

namespace FirePDF.Rendering
{
    public class Rasterizer : Renderer
    {
        private readonly Graphics graphics;

        public Rasterizer(Graphics graphics)
        {
            this.graphics = graphics;
        }

        public override void WillStartRenderingPage(RectangleF boundingBox, Func<Model.GraphicsState> getGraphicsState)
        {
            base.WillStartRenderingPage(boundingBox, getGraphicsState);

            float scale = dpi / 72f;

            Model.GraphicsState graphicsState = getGraphicsState();
            graphicsState.CurrentTransformationMatrix.Translate(0, scale * boundingBox.Height);
            graphicsState.CurrentTransformationMatrix.Scale(scale, -scale);
        }
        
        /// <summary>
        /// refreshes the state of the graphicsContext from the graphics state returned by getGraphicsState
        /// </summary>
        private void RefreshGraphicsState()
        {
            Model.GraphicsState gs = getGraphicsState();

            graphics.Transform = new Matrix();
            graphics.SetClip(gs.clippingPath, CombineMode.Replace);
            
            graphics.Transform = gs.CurrentTransformationMatrix;
        }

        public override void DrawImage(XObjectImage image)
        {
            RefreshGraphicsState();

            Matrix temp = graphics.Transform.Clone();
            
            temp.Scale(1, -1);
            temp.Translate(0, -1);
            
            graphics.Transform = temp;

            try
            {
                graphics.DrawImage(image.GetImage(), 0, 0, 1, 1);
            }
            catch
            {
                Debug.WriteLine("Error drawing image, skipping");
            }

            graphics.Transform = getGraphicsState().CurrentTransformationMatrix;
        }

        public override void FillAndStrokePath(GraphicsPath path)
        {
            FillPath(path);
            StrokePath(path);
        }

        public override void FillPath(GraphicsPath path)
        {
            RefreshGraphicsState();

            Brush b = new SolidBrush(getGraphicsState().nonStrokingColor);
            graphics.FillPath(b, path);
        }

        public override void StrokePath(GraphicsPath path)
        {
            RefreshGraphicsState();

            Model.GraphicsState gs = getGraphicsState();

            Pen p = new Pen(gs.strokingColor, gs.lineWidth);
            graphics.DrawPath(p, path);
        }

        public override void DrawText(byte[] text)
        {
            RefreshGraphicsState();

            Model.GraphicsState gs = getGraphicsState();
            Matrix textRenderingMatrix = new Matrix(gs.fontSize * gs.horizontalScaling, 0, 0, gs.fontSize, 0, gs.textRise);
            textRenderingMatrix.Multiply(gs.textMatrix, MatrixOrder.Append);
            textRenderingMatrix.Multiply(gs.CurrentTransformationMatrix, MatrixOrder.Append);

            graphics.Transform = textRenderingMatrix;

            Matrix temp = graphics.Transform.Clone();

            temp.Scale(1, -1);
            temp.Translate(0, -1);

            graphics.Transform = temp;

            string textString = gs.font.ReadUnicodeStringFromHexString(text);

            graphics.DrawString(textString, new System.Drawing.Font(FontFamily.GenericSerif, 1), new SolidBrush(gs.nonStrokingColor), PointF.Empty);
            graphics.Transform = getGraphicsState().CurrentTransformationMatrix;
        }
    }
}
