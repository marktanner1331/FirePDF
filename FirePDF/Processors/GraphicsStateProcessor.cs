using FirePDF.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FirePDF.Processors
{
    public class GraphicsStateProcessor
    {
        private IStreamOwner streamOwner;
        private Stack<GraphicsState> graphicsStack;
        
        /// <summary>
        /// initializes a new graphics state processor
        /// </summary>
        /// <param name="streamOwner">the graphics state processor will need access to the resources, i.e. for fonts, color spaces etc</param>
        public GraphicsStateProcessor(IStreamOwner streamOwner, Rectangle initialClippingPath)
        {
            this.streamOwner = streamOwner;

            this.graphicsStack = new Stack<GraphicsState>();
            this.graphicsStack.Push(new GraphicsState());
        }

        public GraphicsState getCurrentState()
        {
            return graphicsStack.Peek();
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
