using System;
using System.Collections.Generic;
using System.Drawing;
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

        public LoggingRenderer(Func<GraphicsState> getGraphicsState, IStreamOwner streamOwner, Action<string> logger) : base(getGraphicsState, streamOwner)
        {
            this.logger = logger;
        }

        public override void drawImage(Image image)
        {
            logger.Invoke("drawImage()");
        }

        public override void fillAndStrokePath(Path path, WindingRule windingRule)
        {
            logger.Invoke($"fillAndStrokePath(path, {windingRule})");
        }

        public override void fillPath(Path path, WindingRule windingRule)
        {
            logger.Invoke($"fillPath(path, {windingRule})");
        }

        public override void strokePath(Path path)
        {
            logger.Invoke("strokePath(path)");
        }
    }
}
