using System;
using System.Collections.Generic;
using System.Drawing.Drawing2D;
using System.Text;
using FirePDF.Model;

namespace FirePDF.Rendering
{
    public class NullRenderer : IRenderer
    {
        public override void drawImage(XObjectImage image)
        {
           
        }

        public override void drawText(byte[] text)
        {
            
        }

        public override void fillAndStrokePath(GraphicsPath path)
        {
            
        }

        public override void fillPath(GraphicsPath path)
        {
            
        }

        public override void strokePath(GraphicsPath path)
        {
            
        }
    }
}
