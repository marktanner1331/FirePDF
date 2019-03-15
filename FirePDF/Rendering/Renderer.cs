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
    public class Renderer : IRenderer
    {
        private Graphics graphics;

        public Renderer(Graphics graphicsContext, RectangleF bounds, Func<Model.GraphicsState> getGraphicsState, IStreamOwner streamOwner) : base(getGraphicsState, streamOwner)
        {
            graphics = graphicsContext;

            Model.Rectangle streamBounds = streamOwner.getBoundingBox();

            Model.GraphicsState graphicsState = getGraphicsState();
            graphicsState.currentTransformationMatrix.Translate(0, streamBounds.height);
            graphicsState.currentTransformationMatrix.Scale(1, -1);
        }

        public override void drawImage(Image image)
        {
            throw new NotImplementedException();
        }

        public override void fillAndStrokePath(GraphicsPath path)
        {
            throw new NotImplementedException();
        }

        public override void fillPath(GraphicsPath path)
        {
            Brush b = new SolidBrush(Color.Red);
            graphics.FillPath(b, path);
        }

        public override void strokePath(GraphicsPath path)
        {
            Pen p = new Pen(Color.Blue);
            graphics.DrawPath(p, path);
        }
    }
}
