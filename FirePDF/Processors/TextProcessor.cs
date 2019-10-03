using System;
using System.Collections.Generic;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FirePDF.Model;
using FirePDF.Rendering;
using GraphicsState = FirePDF.Model.GraphicsState;

namespace FirePDF.Processors
{
    class TextProcessor
    {
        private readonly IRenderer renderer;

        private bool isInTextObject = false;
        private readonly Func<Model.GraphicsState> getGraphicsState;

        public TextProcessor(Func<Model.GraphicsState> getGraphicsState, IRenderer renderer)
        {
            this.renderer = renderer;
            this.getGraphicsState = getGraphicsState;
        }

        public bool processOperation(Operation operation)
        {
            switch (operation.operatorName)
            {
                case "BT":
                    isInTextObject = true;

                    GraphicsState g = getGraphicsState();
                    g.textMatrix = new Matrix();
                    g.textLineMatrix = new Matrix();
                    break;
                case "ET":
                    isInTextObject = false;
                    break;
                default:
                    return false;
            }

            return true;
        }
    }
}
