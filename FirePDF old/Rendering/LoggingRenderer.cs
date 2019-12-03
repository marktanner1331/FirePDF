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
    /// logs out all method calls
    /// </summary>
    public class LoggingRenderer : IRenderer
    {
        private Action<string> logger;

        public LoggingRenderer(Action<string> logger)
        {
            this.logger = logger;
        }

        public override void drawImage(XObjectImage image)
        {
            logger("drawImage()");
        }

        public override void drawText(byte[] text)
        {
            logger("drawText()");
        }

        public override void fillAndStrokePath(GraphicsPath path)
        {
            logger("fillAndStrokePath()");
        }

        public override void fillPath(GraphicsPath path)
        {
            logger("fillPath()");
        }

        public override void strokePath(GraphicsPath path)
        {
            logger("strokePath()");
        }
    }
}
