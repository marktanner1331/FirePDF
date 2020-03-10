using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using FirePDF.Model;

namespace FirePDF.Rendering
{
    /// <summary>
    /// renders the bounding boxes of graphics (green) images (blue) and text (red)
    /// does not apply clipping to any of these boxes
    /// </summary>
    public class BoundingBoxRasterizer : Renderer
    {
        private readonly Graphics graphics;
        private readonly bool showGraphics;
        private readonly bool showText;
        private readonly bool showImages;

        public BoundingBoxRasterizer(Graphics graphics, bool showGraphics, bool showText, bool showImages)
        {
            this.graphics = graphics;
            this.showGraphics = showGraphics;
            this.showText = showText;
            this.showImages = showImages;
        }

        public override void WillStartRenderingPage(RectangleF boundingBox, Func<Model.GraphicsState> getGraphicsState)
        {
            base.WillStartRenderingPage(boundingBox, getGraphicsState);

            float scale = dpi / 72f;

            Model.GraphicsState graphicsState = getGraphicsState();
            graphicsState.CurrentTransformationMatrix.Translate(0, scale * boundingBox.Height);
            graphicsState.CurrentTransformationMatrix.Scale(scale, -scale);
        }

        private void RefreshGraphicsState()
        {
            Model.GraphicsState gs = getGraphicsState();
            graphics.Transform = gs.CurrentTransformationMatrix;
        }

        public override void DrawImage(XObjectImage image)
        {
            if(showImages == false)
            {
                return;
            }

            RefreshGraphicsState();

            Matrix temp = graphics.Transform.Clone();

            temp.Scale(1, -1);
            temp.Translate(0, -1);

            graphics.Transform = temp;
            graphics.FillRectangle(new SolidBrush(Color.FromArgb(220, Color.LightBlue)), 0, 0, 1, 1);
            graphics.DrawRectangle(new Pen(Brushes.Black, 1f/image.width), 0, 0, 1, 1);
            graphics.Transform = getGraphicsState().CurrentTransformationMatrix;
        }

        public override void FillAndStrokePath(GraphicsPath path)
        {
            if(showGraphics == false)
            {
                return;
            }

            RefreshGraphicsState();

            RectangleF bounds = path.GetBounds();
            graphics.FillRectangle(new SolidBrush(Color.FromArgb(127, Color.Green)), bounds);
            graphics.DrawRectangle(new Pen(Brushes.Black), bounds.X, bounds.Y, bounds.Width, bounds.Height);
        }

        public override void FillPath(GraphicsPath path)
        {
            if (showGraphics == false)
            {
                return;
            }

            RefreshGraphicsState();

            RectangleF bounds = path.GetBounds();
            graphics.FillRectangle(new SolidBrush(Color.FromArgb(127, Color.Green)), bounds);
            graphics.DrawRectangle(new Pen(Brushes.Black), bounds.X, bounds.Y, bounds.Width, bounds.Height);
        }

        public override void StrokePath(GraphicsPath path)
        {
            if (showGraphics == false)
            {
                return;
            }

            RefreshGraphicsState();

            RectangleF bounds = path.GetBounds();
            graphics.FillRectangle(new SolidBrush(Color.FromArgb(127, Color.Green)), bounds);
            graphics.DrawRectangle(new Pen(Brushes.Black), bounds.X, bounds.Y, bounds.Width, bounds.Height);
        }

        public override void DrawText(byte[] text)
        {
            if(showText == false)
            {
                return;
            }

            RefreshGraphicsState();

            Model.GraphicsState gs = getGraphicsState();
            Model.Font font = gs.font;

            SizeF size = font.MeasureText(text, gs);

            graphics.FillRectangle(new SolidBrush(Color.FromArgb(120, Color.Red)), gs.textMatrix.Elements[4], gs.textMatrix.Elements[5], size.Width, size.Height);
            graphics.DrawRectangle(new Pen(Brushes.Black), gs.textMatrix.Elements[4], gs.textMatrix.Elements[5], size.Width, size.Height);
        }
    }
}
