using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FirePDF.Model;

namespace FirePDF.Rendering
{
    /// <summary>
    /// renders the bounding boxes of graphics (green) images (blue) and text (red)
    /// does not apply clipping to any of these boxes
    /// </summary>
    public class BoundingBoxRasterizer : IRenderer
    {
        private Graphics graphics;
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

        public override void willStartRenderingPage(RectangleF boundingBox, Func<Model.GraphicsState> getGraphicsState)
        {
            base.willStartRenderingPage(boundingBox, getGraphicsState);

            float scale = dpi / 72f;

            Model.GraphicsState graphicsState = getGraphicsState();
            graphicsState.currentTransformationMatrix.Translate(0, scale * boundingBox.Height);
            graphicsState.currentTransformationMatrix.Scale(scale, -scale);
        }

        private void refreshGraphicsState()
        {
            Model.GraphicsState gs = getGraphicsState();
            graphics.Transform = gs.currentTransformationMatrix;
        }

        public override void drawImage(XObjectImage image)
        {
            if(showImages == false)
            {
                return;
            }

            refreshGraphicsState();

            Matrix temp = graphics.Transform.Clone();

            temp.Scale(1, -1);
            temp.Translate(0, -1);

            graphics.Transform = temp;
            graphics.FillRectangle(new SolidBrush(Color.FromArgb(220, Color.LightBlue)), 0, 0, 1, 1);
            graphics.DrawRectangle(new Pen(Brushes.Black, 1f/image.width), 0, 0, 1, 1);
            graphics.Transform = getGraphicsState().currentTransformationMatrix;
        }

        public override void fillAndStrokePath(GraphicsPath path)
        {
            if(showGraphics == false)
            {
                return;
            }

            refreshGraphicsState();

            RectangleF bounds = path.GetBounds();
            graphics.FillRectangle(new SolidBrush(Color.FromArgb(127, Color.Green)), bounds);
            graphics.DrawRectangle(new Pen(Brushes.Black), bounds.X, bounds.Y, bounds.Width, bounds.Height);
        }

        public override void fillPath(GraphicsPath path)
        {
            if (showGraphics == false)
            {
                return;
            }

            refreshGraphicsState();

            RectangleF bounds = path.GetBounds();
            graphics.FillRectangle(new SolidBrush(Color.FromArgb(127, Color.Green)), bounds);
            graphics.DrawRectangle(new Pen(Brushes.Black), bounds.X, bounds.Y, bounds.Width, bounds.Height);
        }

        public override void strokePath(GraphicsPath path)
        {
            if (showGraphics == false)
            {
                return;
            }

            refreshGraphicsState();

            RectangleF bounds = path.GetBounds();
            graphics.FillRectangle(new SolidBrush(Color.FromArgb(127, Color.Green)), bounds);
            graphics.DrawRectangle(new Pen(Brushes.Black), bounds.X, bounds.Y, bounds.Width, bounds.Height);
        }

        public override void drawText(byte[] text)
        {
            if(showText == false)
            {
                return;
            }

            refreshGraphicsState();

            FirePDF.Model.GraphicsState gs = getGraphicsState();
            Model.Font font = gs.font;

            SizeF size = font.measureText(text, gs);

            graphics.FillRectangle(new SolidBrush(Color.FromArgb(120, Color.Red)), gs.textMatrix.Elements[4], gs.textMatrix.Elements[5], size.Width, size.Height);
            graphics.DrawRectangle(new Pen(Brushes.Black), gs.textMatrix.Elements[4], gs.textMatrix.Elements[5], size.Width, size.Height);
        }
    }
}
