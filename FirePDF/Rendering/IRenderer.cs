using FirePDF.Model;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FirePDF.Rendering
{
    public abstract class IRenderer
    {
        public readonly Func<GraphicsState> getGraphicsState;
        public readonly IStreamOwner streamOwner;

        public IRenderer(Func<GraphicsState> getGraphicsState, IStreamOwner streamOwner)
        {
            this.getGraphicsState = getGraphicsState;
            this.streamOwner = streamOwner;
        }

        public abstract void fillAndStrokePath(Path path, WindingRule windingRule);

        public abstract void fillPath(Path path, WindingRule windingRule);

        public abstract void strokePath(Path path);

        public abstract void drawImage(Image image);
    }
}
