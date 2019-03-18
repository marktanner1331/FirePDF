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

        /// <summary>
        /// refreshes the state of the graphicsContext from the graphics state returned by getGraphicsState
        /// </summary>
        private void refreshGraphicsState()
        {
            Model.GraphicsState gs = getGraphicsState();

            graphics.Transform = gs.currentTransformationMatrix;
            graphics.SetClip(gs.clippingPath);
        }

        public override void drawImage(Image image)
        {
            refreshGraphicsState();

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
            refreshGraphicsState();

            Brush b = new SolidBrush(getGraphicsState().nonStrokingColor);
            graphics.FillPath(b, path);
        }

        public override void strokePath(GraphicsPath path)
        {
            refreshGraphicsState();

            Pen p = new Pen(getGraphicsState().strokingColor);
            graphics.DrawPath(p, path);
        }
    }
}
