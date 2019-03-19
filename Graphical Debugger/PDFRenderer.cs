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
            Rasterizer renderer = new Rasterizer(graphics);
            StreamProcessor sp = new StreamProcessor(renderer);
            sp.render(streamOwner, operations);
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);
            renderGraphics(e.Graphics);
        }
    }
}
