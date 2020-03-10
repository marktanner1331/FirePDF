using FirePDF.Model;
using FirePDF.Processors;
using FirePDF.Reading;
using FirePDF.Rendering;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;

namespace Graphical_Debugger
{
    internal class PdfRenderer : Control
    {
        private IStreamOwner streamOwner;
        private List<bool> operationsMap;

        public PdfRenderer()
        {
            
        }

        public void Render(IStreamOwner streamOwner, List<bool> operationsMap)
        {
            this.streamOwner = streamOwner;
            this.operationsMap = operationsMap;

            Invalidate();
        }

        private void RenderGraphics(Graphics graphics)
        {
            graphics.Clear(Color.White);

            if(streamOwner == null)
            {
                return;
            }
            
            Rasterizer renderer = new Rasterizer(graphics);
            StreamProcessor sp = new FilteredStreamProcessor(operationsMap, renderer);
            RecursiveStreamReader streamReader = new RecursiveStreamReader(sp);

            float xScale = ClientSize.Width / streamOwner.BoundingBox.Width;
            float yScale = ClientSize.Height / streamOwner.BoundingBox.Height;
            float scale = Math.Min(xScale, yScale);

            RectangleF box = streamOwner.BoundingBox;
            box.X *= scale;
            box.Y *= scale;
            box.Width *= scale;
            box.Height *= scale;
            graphics.FillRectangle(Brushes.White, box);
            renderer.dpi = (int)(72 * scale);
            
            streamReader.ReadStreamRecursively(streamOwner);
        }

        protected override void OnClientSizeChanged(EventArgs e)
        {
            base.OnClientSizeChanged(e);
            Invalidate();
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);
            RenderGraphics(e.Graphics);
        }
    }
}
