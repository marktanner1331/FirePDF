using System;
using System.Drawing.Drawing2D;
using FirePDF.Model;

namespace FirePDF.Rendering
{
    /// <summary>
    /// logs out all method calls
    /// </summary>
    public class LoggingRenderer : Renderer
    {
        private readonly Action<string> logger;

        public LoggingRenderer(Action<string> logger)
        {
            this.logger = logger;
        }

        public override void DrawImage(XObjectImage image)
        {
            logger("drawImage()");
        }

        public override void DrawText(byte[] text)
        {
            logger("drawText()");
        }

        public override void FillAndStrokePath(GraphicsPath path)
        {
            logger("fillAndStrokePath()");
        }

        public override void FillPath(GraphicsPath path)
        {
            logger("fillPath()");
        }

        public override void StrokePath(GraphicsPath path)
        {
            logger("strokePath()");
        }
    }
}
