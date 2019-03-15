using FirePDF.Model;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FirePDF.Rendering
{
    public abstract class IRenderer
    {
        public readonly Func<Model.GraphicsState> getGraphicsState;
        public readonly IStreamOwner streamOwner;

        public IRenderer(Func<Model.GraphicsState> getGraphicsState, IStreamOwner streamOwner)
        {
            this.getGraphicsState = getGraphicsState;
            this.streamOwner = streamOwner;
        }

        public abstract void fillAndStrokePath(GraphicsPath path);

        public abstract void fillPath(GraphicsPath path);

        public abstract void strokePath(GraphicsPath path);

        public abstract void drawImage(Image image);
    }
}
