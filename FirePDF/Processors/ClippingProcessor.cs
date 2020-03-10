using FirePDF.Model;
using System;
using System.Drawing.Drawing2D;

namespace FirePDF.Processors
{
    public class ClippingProcessor
    {
        private readonly Func<Model.GraphicsState> getGraphicsState;
        private readonly LineProcessor lineProcessor;

        //if this is not null then the current path should be used as a clipping path upon the next painting operation
        //see Pdf 8.5.4 for mor details
        //we can't simply store this in currentPath.windingRule as this could be used for real painting with a different winding rule
        //i.e. the operations could go W F*
        private FillMode? shouldClipPath = null;

        public ClippingProcessor(Func<Model.GraphicsState> getGraphicsState, LineProcessor lineProcessor)
        {
            this.getGraphicsState = getGraphicsState;
            this.lineProcessor = lineProcessor;
        }

        public bool ShouldClipCurrentPath => shouldClipPath != null;

        public static bool IsClippingCommand(string operatorName)
        {
            switch (operatorName)
            {
                case "W":
                case "W*":
                    return true;
                default:
                    return false;
            }
        }

        public bool ProcessOperation(Operation operation)
        {
            switch (operation.operatorName)
            {
                case "W":
                    shouldClipPath = FillMode.Winding;
                    break;
                case "W*":
                    shouldClipPath = FillMode.Alternate;
                    break;
                case "b":
                case "b*":
                case "B":
                case "B*":
                case "f":
                case "F":
                case "f*":
                case "n":
                case "S":
                    if (shouldClipPath != null)
                    {
                        GraphicsPath currentPath = lineProcessor.CurrentPath;

                        currentPath.FillMode = shouldClipPath.Value;
                        getGraphicsState().IntersectClippingPath(currentPath);
                        shouldClipPath = null;
                    }
                    break;
                default:
                    return false;
            }

            return true;
        }
    }
}
