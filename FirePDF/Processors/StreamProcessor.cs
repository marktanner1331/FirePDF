using FirePDF.Model;
using FirePDF.Rendering;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FirePDF.Processors
{
    public class StreamProcessor
    {
        private IRenderer renderer;

        public StreamProcessor(IRenderer renderer)
        {
            this.renderer = renderer;
        }

        public void render(IStreamOwner streamOwner, IEnumerable<Operation> operations)
        {
            //hint: when doing recursive streams, make StreamProcessor a IStreamOwner and initialize the gsp with 'this' in the constructor of StreamProcessor

            GraphicsStateProcessor gsp = new GraphicsStateProcessor(streamOwner);
            LineProcessor lp = new LineProcessor(renderer);
            PaintingProcessor pp = new PaintingProcessor(renderer, lp);
            ClippingProcessor cp = new ClippingProcessor(gsp.getCurrentState, lp);
            ImageProcessor ip = new ImageProcessor(streamOwner, renderer);

            renderer.willStartRenderingPage(gsp.getCurrentState);
            renderer.willStartRenderingStream(streamOwner);

            foreach (Operation operation in operations)
            {
                gsp.processOperation(operation);
                pp.processOperation(operation);
                cp.processOperation(operation);
                lp.processOperation(operation);
                ip.processOperation(operation);
            }
        }
    }
}
