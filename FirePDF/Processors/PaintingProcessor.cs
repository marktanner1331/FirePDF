using FirePDF.Model;
using FirePDF.Rendering;
using System.Drawing.Drawing2D;

namespace FirePDF.Processors
{
    public class PaintingProcessor
    {
        private readonly Renderer renderer;
        private readonly LineProcessor lineProcessor;

        public PaintingProcessor(Renderer renderer, LineProcessor lineProcessor)
        {
            this.renderer = renderer;
            this.lineProcessor = lineProcessor;
        }

        public static bool IsPaintingCommand(string operatorName)
        {
            switch (operatorName)
            {
                case "b":
                case "b*":
                case "B":
                case "B*":
                case "f":
                case "F":
                case "f*":
                case "n":
                case "S":
                    return true;
                default:
                    return false;
            }
        }

        public bool ProcessOperation(Operation operation)
        {
            switch (operation.operatorName)
            {
                case "b":
                    lineProcessor.ProcessOperation(new Operation("h", null));
                    ProcessOperation(new Operation("B", null));
                    break;
                case "b*":
                    lineProcessor.ProcessOperation(new Operation("h", null));
                    ProcessOperation(new Operation("B*", null));
                    break;
                case "B":
                    lineProcessor.CurrentPath.FillMode = FillMode.Winding;
                    renderer?.FillAndStrokePath(lineProcessor.CurrentPath);
                    break;
                case "B*":
                    lineProcessor.CurrentPath.FillMode = FillMode.Alternate;
                    renderer?.FillAndStrokePath(lineProcessor.CurrentPath);
                    break;
                case "f":
                case "F":
                    lineProcessor.CurrentPath.FillMode = FillMode.Winding;
                    renderer?.FillPath(lineProcessor.CurrentPath);
                    break;
                case "f*":
                    lineProcessor.CurrentPath.FillMode = FillMode.Alternate;
                    renderer?.FillPath(lineProcessor.CurrentPath);
                    break;
                case "s":
                    lineProcessor.ProcessOperation(new Operation("h", null));
                    ProcessOperation(new Operation("S", operation.operands));
                    break;
                case "S":
                    renderer?.StrokePath(lineProcessor.CurrentPath);
                    break;
                default:
                    return false;
            }

            return true;
        }
    }
}
