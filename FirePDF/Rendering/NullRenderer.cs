using System.Drawing.Drawing2D;
using FirePDF.Model;

namespace FirePDF.Rendering
{
    public class NullRenderer : Renderer
    {
        public override void DrawImage(XObjectImage image)
        {
           
        }

        public override void DrawText(byte[] text)
        {
            
        }

        public override void FillAndStrokePath(GraphicsPath path)
        {
            
        }

        public override void FillPath(GraphicsPath path)
        {
            
        }

        public override void StrokePath(GraphicsPath path)
        {
            
        }
    }
}
