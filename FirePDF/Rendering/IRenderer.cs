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
        public Func<Model.GraphicsState> getGraphicsState;
        public IStreamOwner streamOwner;
        public int dpi = 72;
        
        public virtual void willStartRenderingPage(Func<Model.GraphicsState> getGraphicsState)
        {
            this.getGraphicsState = getGraphicsState;
        }

        public virtual void willStartRenderingStream(IStreamOwner streamOwner)
        {
            this.streamOwner = streamOwner;
        }

        public abstract void fillAndStrokePath(GraphicsPath path);

        public abstract void fillPath(GraphicsPath path);

        public abstract void strokePath(GraphicsPath path);

        public abstract void drawImage(XObjectImage image);
    }
}
