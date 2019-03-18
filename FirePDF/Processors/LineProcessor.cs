using FirePDF.Model;
using FirePDF.Rendering;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FirePDF.Processors
{
    /// <summary>
    /// processes drawing commands to do with lines and curves
    /// </summary>
    public class LineProcessor
    {
        private Func<Model.GraphicsState> getGraphicsState;
        private IRenderer renderer;

        private PointF? currentPoint;
        private GraphicsPath currentPath;

        //if this is not null then the current path should be used as a clipping path upon the next painting operation
        //see PDF 8.5.4 for mor details
        //we can't simply store this in currentPath.windingRule as this could be used for real painting with a different winding rule
        //i.e. the operations could go W F*
        FillMode? shouldClipPath = null;

        public LineProcessor(Func<Model.GraphicsState> getGraphicsState, IRenderer renderer)
        {
            this.getGraphicsState = getGraphicsState;
            this.renderer = renderer;
            this.currentPath = new GraphicsPath();
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
                    currentPath.FillMode = FillMode.Winding;
                    renderer.fillAndStrokePath(currentPath);
                    endPath();
                    break;
                case "B*":
                    currentPath.FillMode = FillMode.Alternate;
                    renderer.fillAndStrokePath(currentPath);
                    endPath();
                    break;
                case "c":
                    {
                        PointF[] points = operation.getOperationsAsPointFs();
                        
                        if (currentPoint == null)
                        {
                            logWarning("curveTo (" + points[2].X + "," + points[2].Y + ") without initial MoveTo");
                            currentPoint = points[2];
                        }
                        else
                        {
                            currentPath.AddCurve(new PointF[] { currentPoint.Value, points[0], points[1], points[2] });
                            currentPoint = points[2];
                        }
                    }
                    break;
                case "f":
                case "F":
                    currentPath.FillMode = FillMode.Winding;
                    renderer.fillPath(currentPath);
                    endPath();
                    break;
                case "f*":
                    currentPath.FillMode = FillMode.Alternate;
                    renderer.fillPath(currentPath);
                    endPath();
                    break;
                case "h":
                    if (currentPoint == null)
                    {
                        logWarning("ClosePath without initial MoveTo");
                        return;
                    }

                    currentPath.CloseFigure();
                    break;
                case "l":
                    {
                        PointF[] points = operation.getOperationsAsPointFs();
                        
                        if (currentPoint == null)
                        {
                            logWarning("lineTo (" + points[0].X + "," + points[0].Y + ") without initial MoveTo");
                            currentPoint = points[0];
                        }
                        else
                        {
                            currentPath.AddLine(currentPoint.Value, points[0]);
                            currentPoint = points[0];
                        }
                    }
                    break;
                case "m":
                    {
                        PointF[] points = operation.getOperationsAsPointFs();
                        currentPoint = points[0];
                    }
                    break;
                case "n":
                    endPath();
                    break;
                case "re":
                    {
                        PointF[] points = operation.getOperationsAsPointFs();

                        PointF bottomLeft = points[0];
                        PointF topRight = points[1];

                        //we go from topRight storing the width and height to it storing the upper right point
                        topRight.X += bottomLeft.X;
                        topRight.Y += bottomLeft.Y;
                        
                        // to ensure that the path is created in the right direction, we have to create
                        // it by combining single lines instead of creating a simple rectangle
                        currentPath.AddLine(bottomLeft, new PointF(topRight.X, bottomLeft.Y));
                        currentPath.AddLine(new PointF(topRight.X, bottomLeft.Y), topRight);
                        currentPath.AddLine(topRight, new PointF(bottomLeft.X, topRight.Y));

                        // close the subpath instead of adding the last line so that a possible set line
                        // cap style isn't taken into account at the "beginning" of the rectangle
                        currentPath.CloseFigure();

                        currentPoint = bottomLeft;
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
                        PointF[] points = operation.getOperationsAsPointFs();
                        
                        if (currentPoint == null)
                        {
                            logWarning("curveTo (" + points[1].X + "," + points[1].Y + ") without initial MoveTo");
                            currentPoint = points[1];
                        }
                        else
                        {
                            currentPath.AddCurve(new PointF[] { currentPoint.Value, currentPoint.Value, points[0], points[1] });
                            currentPoint = points[1];
                        }
                        break;
                    }
                case "W":
                    shouldClipPath = FillMode.Winding;
                    break;
                case "W*":
                    shouldClipPath = FillMode.Alternate;
                    break;
                case "y":
                    {
                        PointF[] points = operation.getOperationsAsPointFs();
                        
                        if (currentPoint == null)
                        {
                            logWarning("curveTo (" + points[1].X + "," + points[1].Y + ") without initial MoveTo");
                            currentPoint = points[1];
                        }
                        else
                        {
                            currentPath.AddCurve(new PointF[] { currentPoint.Value, points[0], points[1], points[1] });
                            currentPoint = points[1];
                        }
                        break;
                    }
            }
        }

        private void endPath()
        {
            if (shouldClipPath != null)
            {
                currentPath.FillMode = shouldClipPath.Value;
                getGraphicsState().intersectClippingPath(currentPath);
                shouldClipPath = null;
            }

            currentPath.Reset();
        }

        private void logWarning(string warning)
        {
            Debug.WriteLine(warning);
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

        private IEnumerable<PointF> convertFloatsToCoordinates(IEnumerable<float> coordinates)
        {
            IEnumerator<float> enumerator = coordinates.GetEnumerator();

            while (enumerator.MoveNext())
            {
                float x = enumerator.Current;
                enumerator.MoveNext();

                float y = enumerator.Current;

                yield return new PointF(x, y);
            }
        }

        private void scaleListOfCoordinatesByCTM(List<float> coordinates)
        {
            PointF[] points = convertFloatsToCoordinates(coordinates).ToArray();
            getGraphicsState().currentTransformationMatrix.TransformPoints(points);

            for (int i = 0; i < points.Length; i++)
            {
                int j = i * 2;
                coordinates[j] = points[i].X;
                coordinates[j + 1] = points[i].Y;
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
