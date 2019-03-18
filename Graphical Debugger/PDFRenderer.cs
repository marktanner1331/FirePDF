using FirePDF.Model;
using FirePDF.Processors;
using FirePDF.Rendering;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Graphical_Debugger
{
    class PDFRenderer : Control
    {
        private IStreamOwner streamOwner;
        private IEnumerable<Operation> operations;

        public PDFRenderer()
        {
            
        }

        public void render(IStreamOwner streamOwner, IEnumerable<Operation> operations)
        {
            this.streamOwner = streamOwner;
            this.operations = operations;

            Invalidate();
        }

        private void renderGraphics(Graphics graphics)
        {
            GraphicsStateProcessor gsp = new GraphicsStateProcessor(streamOwner);

            RectangleF bounds = new RectangleF(0, 0, Width, Height);
            Renderer renderer = new Renderer(graphics, bounds, gsp.getCurrentState, streamOwner);

            LineProcessor lp = new LineProcessor(gsp.getCurrentState, renderer);
            ImageProcessor ip = new ImageProcessor(streamOwner, renderer);

            foreach (Operation operation in operations)
            {
                gsp.processOperation(operation);
                lp.processOperation(operation);
                ip.processOperation(operation);
            }
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);
            renderGraphics(e.Graphics);
        }
    }
}
