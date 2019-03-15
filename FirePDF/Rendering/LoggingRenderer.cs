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

        public LoggingRenderer(Func<Model.GraphicsState> getGraphicsState, IStreamOwner streamOwner, Action<string> logger) : base(getGraphicsState, streamOwner)
        {
            this.logger = logger;
        }

        public override void drawImage(Image image)
        {
            logger.Invoke("drawImage()");
        }

        public override void fillAndStrokePath(GraphicsPath path)
        {
            logger.Invoke("fillAndStrokePath(path)");
        }

        public override void fillPath(GraphicsPath path)
        {
            logger.Invoke("fillPath(path)");
        }

        public override void strokePath(GraphicsPath path)
        {
            logger.Invoke("strokePath(path)");
        }
    }
}
