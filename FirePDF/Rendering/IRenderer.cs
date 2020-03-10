using FirePDF.Model;
using System;
using System.Drawing;
using System.Drawing.Drawing2D;

namespace FirePDF.Rendering
{
    public abstract class Renderer
    {
        public Func<Model.GraphicsState> getGraphicsState;
        public IStreamOwner streamOwner;
        public int dpi = 72;
        
        public virtual void WillStartRenderingPage(RectangleF boundingBox, Func<Model.GraphicsState> getGraphicsState)
        {
            this.getGraphicsState = getGraphicsState;
        }

        public virtual void WillStartRenderingStream(IStreamOwner streamOwner)
        {
            this.streamOwner = streamOwner;
        }

        public abstract void FillAndStrokePath(GraphicsPath path);

        public abstract void FillPath(GraphicsPath path);

        public abstract void StrokePath(GraphicsPath path);

        public abstract void DrawImage(XObjectImage image);

        //TODO: convert this PDFString
        public abstract void DrawText(byte[] text);
    }
}
