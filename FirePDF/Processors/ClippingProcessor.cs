using FirePDF.Model;
using System;
using System.Collections.Generic;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FirePDF.Processors
{
    public class ClippingProcessor
    {
        private Func<Model.GraphicsState> getGraphicsState;
        private LineProcessor lineProcessor;

        //if this is not null then the current path should be used as a clipping path upon the next painting operation
        //see PDF 8.5.4 for mor details
        //we can't simply store this in currentPath.windingRule as this could be used for real painting with a different winding rule
        //i.e. the operations could go W F*
        private FillMode? shouldClipPath = null;

        public ClippingProcessor(Func<Model.GraphicsState> getGraphicsState, LineProcessor lineProcessor)
        {
            this.getGraphicsState = getGraphicsState;
            this.lineProcessor = lineProcessor;
        }

        public bool shouldClipCurrentPath => shouldClipPath != null;
        
        public bool processOperation(Operation operation)
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
                        GraphicsPath currentPath = lineProcessor.currentPath;

                        currentPath.FillMode = shouldClipPath.Value;
                        getGraphicsState().intersectClippingPath(currentPath);
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
