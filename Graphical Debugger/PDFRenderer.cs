using FirePDF.Model;
using FirePDF.Processors;
using FirePDF.Rendering;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
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

            float xScale = ClientSize.Width / streamOwner.boundingBox.Width;
            float yScale = ClientSize.Height / streamOwner.boundingBox.Height;
            float scale = Math.Min(xScale, yScale);

            RectangleF box = streamOwner.boundingBox;
            box.X *= scale;
            box.Y *= scale;
            box.Width *= scale;
            box.Height *= scale;
            graphics.FillRectangle(Brushes.White, box);
            renderer.dpi = (int)(72 * scale);

            sp.render(streamOwner, operations);
        }

        protected override void OnClientSizeChanged(EventArgs e)
        {
            base.OnClientSizeChanged(e);
            Invalidate();
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);
            renderGraphics(e.Graphics);
        }
    }
}
