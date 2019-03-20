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
        private PointF? currentPoint;
        public GraphicsPath currentPath { get; private set; }
        
        public LineProcessor()
        {
            this.currentPath = new GraphicsPath();
        }

        /// <summary>
        /// returns true for all operators that append paths to the current path
        /// but returns false for painting commands that reset the current path (including 'n')
        /// </summary>
        public static bool isPathDrawingCommand(string operatorName)
        {
            switch (operatorName)
            {
                case "c":
                case "h":
                case "l":
                case "m":
                case "re":
                case "v":
                case "y":
                    return true;
                default:
                    return false;
            }
        }

        public bool processOperation(Operation operation)
        {
            switch (operation.operatorName)
            {
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
                            currentPath.AddBezier(currentPoint.Value, points[0], points[1], points[2]);
                            currentPoint = points[2];
                        }
                    }
                    break;
                case "h":
                    if (currentPoint == null)
                    {
                        logWarning("ClosePath without initial MoveTo");
                    }
                    else
                    {
                        currentPath.CloseFigure();
                    }
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

                        currentPath.CloseFigure();
                    }
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
                            currentPath.AddBezier(currentPoint.Value, currentPoint.Value, points[0], points[1]);
                            currentPoint = points[1];
                        }
                        break;
                    }
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
                            currentPath.AddBezier(currentPoint.Value, points[0], points[1], points[1]);
                            currentPoint = points[1];
                        }
                        break;
                    }
                case "b":
                case "b*":
                case "B":
                case "B*":
                case "f":
                case "F":
                case "f*":
                case "n":
                case "s":
                case "S":
                    currentPath.Reset();
                    break;
                default:
                    return false;
            }

            return true;
        }

        private void logWarning(string warning)
        {
            Debug.WriteLine(warning);
        }
    }
}
