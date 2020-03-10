using FirePDF.Model;
using FirePDF.Reading;
using FirePDF.Rendering;

namespace FirePDF.Processors
{
    public class StreamProcessor : IStreamProcessor
    {
        private readonly Renderer renderer;
        private GraphicsStateProcessor gsp;
        private LineProcessor lp;
        private PaintingProcessor pp;
        private ClippingProcessor cp;
        private ImageProcessor ip;
        private TextProcessor tp;

        private RecursiveStreamReader streamReader;

        public StreamProcessor(Renderer renderer)
        {
            this.renderer = renderer;
        }

        public void DidStartReadingStream(IStreamOwner streamOwner)
        {
            renderer.WillStartRenderingStream(streamOwner);
        }
        
        public virtual void ProcessOperation(Operation operation)
        {
            gsp.ProcessOperation(operation);
            pp.ProcessOperation(operation);
            cp.ProcessOperation(operation);
            lp.ProcessOperation(operation);
            ip.ProcessOperation(operation);
            tp.ProcessOperation(operation);
        }
       
        public void WillFinishReadingPage()
        {
            
        }

        public void WillFinishReadingStream()
        {
            
        }

        public void WillStartReadingPage(RecursiveStreamReader parser)
        {
            streamReader = parser;
            
            gsp = new GraphicsStateProcessor(() => parser.Resources, parser.BoundingBox);
            lp = new LineProcessor();
            pp = new PaintingProcessor(renderer, lp);
            cp = new ClippingProcessor(gsp.GetCurrentState, lp);
            ip = new ImageProcessor(() => parser.Resources, renderer);
            tp = new TextProcessor(gsp.GetCurrentState, () => parser.Resources, renderer);

            renderer.WillStartRenderingPage(parser.BoundingBox, gsp.GetCurrentState);
        }
    }
}
