using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FirePDF
{
    class GraphicsStateProcessor
    {
        private IStreamProcessor streamProcessor;
        private Stack<GraphicsState> graphicsStack;
        
        /// <summary>
        /// initializes a new graphics state processor
        /// </summary>
        /// <param name="streamProcessor">the graphics state processor will need access to the resources, i.e. for fonts, color spaces etc</param>
        public GraphicsStateProcessor(IStreamProcessor streamProcessor, RectangleF initialClippingPath)
        {
            this.streamProcessor = streamProcessor;
            this.graphicsStack = new Stack<GraphicsState>();
        }

        public void processOperation(Operation operation)
        {
            switch(operation.operatorName)
            {
                case "q":
                    graphicsStack.Push(graphicsStack.Peek().clone());
                    break;
                case "Q":
                    if (graphicsStack.Count > 1)
                    {
                        graphicsStack.Pop();
                    }
                    else
                    {
                        throw new Exception("graphics stack is empty, cannot pop");
                    }
                    break;
            }
        }
    }
}
