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
        public readonly Bitmap image;
        private Graphics graphics;

        public Renderer(Func<Model.GraphicsState> getGraphicsState, IStreamOwner streamOwner) : base(getGraphicsState, streamOwner)
        {
            Model.Rectangle streamSize = streamOwner.getBoundingBox();

            image = new Bitmap((int)Math.Ceiling(streamSize.width), (int)Math.Ceiling(streamSize.height));
            graphics = Graphics.FromImage(image);
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
