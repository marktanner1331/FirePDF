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
        private Model.Rectangle streamBounds;

        public Renderer(Graphics graphicsContext, RectangleF bounds, Func<Model.GraphicsState> getGraphicsState, IStreamOwner streamOwner) : base(getGraphicsState, streamOwner)
        {
            graphics = graphicsContext;

            streamBounds = streamOwner.getBoundingBox();

            Model.GraphicsState graphicsState = getGraphicsState();
            graphicsState.currentTransformationMatrix.Translate(0, streamBounds.height);
            graphicsState.currentTransformationMatrix.Scale(1, -1);
        }

        public override void drawImage(Image image)
        {
            graphics.Transform = getGraphicsState().currentTransformationMatrix;
            Matrix temp = graphics.Transform.Clone();

            temp.Scale(1, -1);
            temp.Translate(0, -1);
            
            graphics.Transform = temp;
            graphics.DrawImage(image, 0, 0, 1, 1);
            graphics.Transform = getGraphicsState().currentTransformationMatrix;
        }

        public override void fillAndStrokePath(GraphicsPath path)
        {
            throw new NotImplementedException();
        }

        public override void fillPath(GraphicsPath path)
        {
            graphics.Transform = getGraphicsState().currentTransformationMatrix;
            Brush b = new SolidBrush(getGraphicsState().nonStrokingColor);
            graphics.FillPath(b, path);
        }

        public override void strokePath(GraphicsPath path)
        {
            graphics.Transform = getGraphicsState().currentTransformationMatrix;
            Pen p = new Pen(getGraphicsState().strokingColor);
            graphics.DrawPath(p, path);
        }
    }
}
