using FirePDF.Model;
using FirePDF.Rendering;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FirePDF.Processors
{
    /// <summary>
    /// processes drawing commands to do with lines and curves
    /// all coordinates are transformed to device space before the renderer is called
    /// </summary>
    public class LineProcessor
    {
        private Func<GraphicsState> getGraphicsState;
        private IRenderer renderer;
        private Path currentPath;

        //if this is not null then the current path should be used as a clipping path upon the next painting operation
        //see PDF 8.5.4 for mor details
        //we can't simply store this in currentPath.windingRule as this could be used for real painting with a different winding rule
        //i.e. the operations could go W F*
        WindingRule? shouldClipPath = null;

        public LineProcessor(Func<GraphicsState> getGraphicsState, IRenderer renderer)
        {
            this.getGraphicsState = getGraphicsState;
            this.renderer = renderer;
            this.currentPath = new Path();
        }

        public void processOperation(Operation operation)
        {
            switch (operation.operatorName)
            {
                case "b":
                    processOperation(new Operation("h", null));
                    processOperation(new Operation("B", null));
                    break;
                case "b*":
                    processOperation(new Operation("h", null));
                    processOperation(new Operation("B*", null));
                    break;
                case "B":
                    renderer.fillAndStrokePath(currentPath, WindingRule.NON_ZERO);
                    endPath();
                    break;
                case "B*":
                    renderer.fillAndStrokePath(currentPath, WindingRule.EVEN_ODD);
                    endPath();
                    break;
                case "c":
                    {
                        List<double> operands = operation.getOperationsAsDoubles();
                        scaleListOfCoordinatesByCTM(operands);

                        if (currentPath.getCurrentPoint() == null)
                        {
                            logWarning("curveTo (" + operands[4] + "," + operands[5] + ") without initial MoveTo");
                            currentPath.moveTo(operands[4], operands[5]);
                        }
                        else
                        {
                            currentPath.cubicCurveTo(operands.ToArray());
                        }
                    }
                    break;
                case "f":
                case "F":
                    renderer.fillPath(currentPath, WindingRule.NON_ZERO);
                    endPath();
                    break;
                case "f*":
                    renderer.fillPath(currentPath, WindingRule.EVEN_ODD);
                    endPath();
                    break;
                case "h":
                    if (currentPath.getCurrentPoint() == null)
                    {
                        logWarning("ClosePath without initial MoveTo");
                        return;
                    }

                    endPath();
                    break;
                case "l":
                    {
                        List<double> operands = operation.getOperationsAsDoubles();
                        scaleListOfCoordinatesByCTM(operands);
                        
                        if (currentPath.getCurrentPoint() == null)
                        {
                            logWarning("lineTo (" + operands[0] + "," + operands[1] + ") without initial MoveTo");
                            currentPath.moveTo(operands);
                        }
                        else
                        {
                            currentPath.lineTo(operands);
                        }
                    }
                    break;
                case "m":
                    {
                        List<double> operands = operation.getOperationsAsDoubles();
                        scaleListOfCoordinatesByCTM(operands);

                        currentPath.moveTo(operands);
                    }
                    break;
                case "n":
                    endPath();
                    break;
                case "re":
                    {
                        List<double> operands = operation.getOperationsAsDoubles();

                        //we go from [2] and [3] storing the width and height to it storing the upper right point
                        operands[2] += operands[0];
                        operands[3] += operands[1];

                        scaleListOfCoordinatesByCTM(operands);

                        // to ensure that the path is created in the right direction, we have to create
                        // it by combining single lines instead of creating a simple rectangle
                        currentPath.moveTo(operands[0], operands[1]);
                        currentPath.lineTo(operands[2], operands[1]);
                        currentPath.lineTo(operands[2], operands[3]);
                        currentPath.lineTo(operands[0], operands[3]);

                        // close the subpath instead of adding the last line so that a possible set line
                        // cap style isn't taken into account at the "beginning" of the rectangle
                        currentPath.closePath();
                        break;
                    }
                case "s":
                    processOperation(new Operation("h", operation.operands));
                    processOperation(new Operation("S", operation.operands));
                    break;
                case "S":
                    renderer.strokePath(currentPath);
                    endPath();
                    break;
                case "v":
                    {
                        List<double> operands = operation.getOperationsAsDoubles();
                        scaleListOfCoordinatesByCTM(operands);
                        
                        PointD currentPoint = currentPath.getCurrentPoint();
                        if (currentPoint == null)
                        {
                            logWarning("curveTo (" + operands[2] + "," + operands[3] + ") without initial MoveTo");
                            currentPath.moveTo(operands[2], operands[3]);
                        }
                        else
                        {
                            currentPath.cubicCurveTo(currentPoint.x, currentPoint.y, operands[0], operands[1], operands[2], operands[3]);
                        }
                        break;
                    }
                case "W":
                    shouldClipPath = WindingRule.NON_ZERO;
                    break;
                case "W*":
                    shouldClipPath = WindingRule.EVEN_ODD;
                    break;
                case "y":
                    {
                        List<double> operands = operation.getOperationsAsDoubles();
                        scaleListOfCoordinatesByCTM(operands);

                        if (currentPath.getCurrentPoint() == null)
                        {
                            logWarning("curveTo (" + operands[2] + "," + operands[3] + ") without initial MoveTo");
                            currentPath.moveTo(operands[2], operands[3]);
                        }
                        else
                        {
                            currentPath.cubicCurveTo(operands[0], operands[1], operands[2], operands[3], operands[2], operands[3]);
                        }
                        break;
                    }
            }
        }

        private void endPath()
        {
            if (shouldClipPath != null)
            {
                currentPath.windingRule = shouldClipPath.Value;
                getGraphicsState().intersectClippingPath(currentPath);
                shouldClipPath = null;
            }

            currentPath.reset();
        }

        private void logWarning(string warning)
        {

        }

        /// <summary>
        /// converts a collection of doubles stored as x1, y1, x2, y2 ... into a collection of PointF's
        /// there must be an even amount of doubles
        /// </summary>
        private IEnumerable<PointF> convertDoublesToCoordinates(IEnumerable<double> coordinates)
        {
            IEnumerator<double> enumerator = coordinates.GetEnumerator();

            while (enumerator.MoveNext())
            {
                double x = enumerator.Current;
                enumerator.MoveNext();

                double y = enumerator.Current;

                yield return new PointF((float)x, (float)y);
            }
        }

        private void scaleListOfCoordinatesByCTM(List<double> coordinates)
        {
            PointF[] points = convertDoublesToCoordinates(coordinates).ToArray();
            getGraphicsState().currentTransformationMatrix.TransformPoints(points);

            for (int i = 0; i < points.Length; i++)
            {
                int j = i * 2;
                coordinates[j] = points[i].X;
                coordinates[j + 1] = points[i].Y;
            }
        }
    }
}
