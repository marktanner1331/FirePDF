using FirePDF.Model;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Drawing2D;

namespace FirePDF.Processors
{
    /// <summary>
    /// processes drawing commands to do with lines and curves
    /// </summary>
    public class LineProcessor
    {
        private PointF? currentPoint;
        public GraphicsPath CurrentPath { get; }
        
        public LineProcessor()
        {
            CurrentPath = new GraphicsPath();
        }

        /// <summary>
        /// returns true for all operators that append paths to the current path
        /// but returns false for painting commands that reset the current path (including 'n')
        /// </summary>
        public static bool IsPathDrawingCommand(string operatorName)
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

        public bool ProcessOperation(Operation operation)
        {
            switch (operation.operatorName)
            {
                case "c":
                    {
                        PointF[] points = operation.GetOperandsAsPointFs();
                        
                        if (currentPoint == null)
                        {
                            LogWarning("curveTo (" + points[2].X + "," + points[2].Y + ") without initial MoveTo");
                            currentPoint = points[2];
                        }
                        else
                        {
                            CurrentPath.AddBezier(currentPoint.Value, points[0], points[1], points[2]);
                            currentPoint = points[2];
                        }
                    }
                    break;
                case "h":
                    if (currentPoint == null)
                    {
                        LogWarning("ClosePath without initial MoveTo");
                    }
                    else
                    {
                        CurrentPath.CloseFigure();
                    }
                    break;
                case "l":
                    {
                        PointF[] points = operation.GetOperandsAsPointFs();

                        if (currentPoint == null)
                        {
                            LogWarning("lineTo (" + points[0].X + "," + points[0].Y + ") without initial MoveTo");
                            currentPoint = points[0];
                        }
                        else
                        {
                            CurrentPath.AddLine(currentPoint.Value, points[0]);
                            currentPoint = points[0];
                        }
                    }
                    break;
                case "m":
                    {
                        PointF[] points = operation.GetOperandsAsPointFs();
                        currentPoint = points[0];

                        CurrentPath.CloseFigure();
                    }
                    break;
                case "re":
                    {
                        PointF[] points = operation.GetOperandsAsPointFs();

                        PointF bottomLeft = points[0];
                        PointF topRight = points[1];

                        //we go from topRight storing the width and height to it storing the upper right point
                        topRight.X += bottomLeft.X;
                        topRight.Y += bottomLeft.Y;
                        
                        // to ensure that the path is created in the right direction, we have to create
                        // it by combining single lines instead of creating a simple rectangle
                        CurrentPath.AddLine(bottomLeft, new PointF(topRight.X, bottomLeft.Y));
                        CurrentPath.AddLine(new PointF(topRight.X, bottomLeft.Y), topRight);
                        CurrentPath.AddLine(topRight, new PointF(bottomLeft.X, topRight.Y));

                        // close the subpath instead of adding the last line so that a possible set line
                        // cap style isn't taken into account at the "beginning" of the rectangle
                        CurrentPath.CloseFigure();

                        currentPoint = bottomLeft;
                        break;
                    }
                case "v":
                    {
                        PointF[] points = operation.GetOperandsAsPointFs();
                        
                        if (currentPoint == null)
                        {
                            LogWarning("curveTo (" + points[1].X + "," + points[1].Y + ") without initial MoveTo");
                            currentPoint = points[1];
                        }
                        else
                        {
                            CurrentPath.AddBezier(currentPoint.Value, currentPoint.Value, points[0], points[1]);
                            currentPoint = points[1];
                        }
                        break;
                    }
                case "y":
                    {
                        PointF[] points = operation.GetOperandsAsPointFs();
                        
                        if (currentPoint == null)
                        {
                            LogWarning("curveTo (" + points[1].X + "," + points[1].Y + ") without initial MoveTo");
                            currentPoint = points[1];
                        }
                        else
                        {
                            CurrentPath.AddBezier(currentPoint.Value, points[0], points[1], points[1]);
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
                    CurrentPath.Reset();
                    break;
                default:
                    return false;
            }

            return true;
        }

        private static void LogWarning(string warning)
        {
            Debug.WriteLine(warning);
        }
    }
}
