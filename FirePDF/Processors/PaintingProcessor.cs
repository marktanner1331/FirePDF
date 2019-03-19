using FirePDF.Model;
using FirePDF.Rendering;
using System;
using System.Collections.Generic;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FirePDF.Processors
{
    public class PaintingProcessor
    {
        private IRenderer renderer;
        private LineProcessor lineProcessor;

        public PaintingProcessor(IRenderer renderer, LineProcessor lineProcessor)
        {
            this.renderer = renderer;
            this.lineProcessor = lineProcessor;
        }

        public bool processOperation(Operation operation)
        {
            switch (operation.operatorName)
            {
                case "b":
                    lineProcessor.processOperation(new Operation("h", null));
                    processOperation(new Operation("B", null));
                    break;
                case "b*":
                    lineProcessor.processOperation(new Operation("h", null));
                    processOperation(new Operation("B*", null));
                    break;
                case "B":
                    lineProcessor.currentPath.FillMode = FillMode.Winding;
                    renderer?.fillAndStrokePath(lineProcessor.currentPath);
                    break;
                case "B*":
                    lineProcessor.currentPath.FillMode = FillMode.Alternate;
                    renderer?.fillAndStrokePath(lineProcessor.currentPath);
                    break;
                case "f":
                case "F":
                    lineProcessor.currentPath.FillMode = FillMode.Winding;
                    renderer?.fillPath(lineProcessor.currentPath);
                    break;
                case "f*":
                    lineProcessor.currentPath.FillMode = FillMode.Alternate;
                    renderer?.fillPath(lineProcessor.currentPath);
                    break;
                case "s":
                    lineProcessor.processOperation(new Operation("h", null));
                    processOperation(new Operation("S", operation.operands));
                    break;
                case "S":
                    renderer?.strokePath(lineProcessor.currentPath);
                    break;
                default:
                    return false;
            }

            return true;
        }
    }
}
