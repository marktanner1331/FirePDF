﻿using FirePDF.Model;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Matrix = System.Drawing.Drawing2D.Matrix;
using GraphicsPath = System.Drawing.Drawing2D.GraphicsPath;

namespace FirePDF.Processors
{
    public class GraphicsStateProcessor
    {
        private Func<PDFResources> getResources;
        private Stack<GraphicsState> graphicsStack;

        /// <summary>
        /// initializes a new graphics state processor
        /// </summary>
        /// <param name="getResources">the graphics state processor will need access to the resources, i.e. for fonts, color spaces etc</param>
        public GraphicsStateProcessor(Func<PDFResources> getResources, RectangleF boundingBox)
        {
            this.getResources = getResources;

            this.graphicsStack = new Stack<GraphicsState>();

            GraphicsPath clippingPath = new GraphicsPath();
            clippingPath.AddRectangle(boundingBox);
            this.graphicsStack.Push(new GraphicsState(clippingPath));
        }

        /// <summary>
        /// initializes a new graphics state processor
        /// </summary>
        /// <param name="getResources">the graphics state processor will need access to the resources, i.e. for fonts, color spaces etc</param>
        public GraphicsStateProcessor(Func<PDFResources> getResources, GraphicsPath initialClippingPath)
        {
            this.getResources = getResources;

            this.graphicsStack = new Stack<GraphicsState>();
            this.graphicsStack.Push(new GraphicsState(initialClippingPath));
        }

        public GraphicsState getCurrentState()
        {
            return graphicsStack.Peek();
        }

        public void processOperation(Operation operation)
        {
            switch (operation.operatorName)
            {
                case "cm":
                    {
                        List<float> operands = operation.getOperandsAsFloats();

                        Matrix matrix = new Matrix(operands[0], operands[1], operands[2], operands[3], operands[4], operands[5]);
                        getCurrentState().currentTransformationMatrix.Multiply(matrix);
                        break;
                    }
                //                case "CS":
                //                    {
                //                        COSName name = (COSName)operands.get(0);
                //                        PDColorSpace cs = renderer.getResources().getColorSpace(name);

                //                        getGraphicsState().setStrokingColorSpace(cs);
                //                        getGraphicsState().setStrokingColor(cs.getInitialColor());
                //                        break;
                //                    }
                //                case "cs":
                //                    {
                //                        COSName name = (COSName)operands.get(0);
                //                        PDColorSpace cs = renderer.getResources().getColorSpace(name);

                //                        getGraphicsState().setNonStrokingColorSpace(cs);
                //                        getGraphicsState().setNonStrokingColor(cs.getInitialColor());
                //                        break;
                //                    }
                //                case "d":
                //                    processLineDashPattern(operator, operands);
                //                    break;
                //                case "G":
                //                    {
                //                        PDColorSpace cs = renderer.getResources().getColorSpace(COSName.DEVICEGRAY);
                //                        getGraphicsState().setStrokingColorSpace(cs);

                //                        setStrokingColor(operator, operands);
                //                        break;
                //                    }
                //                case "g":
                //                    {
                //                        PDColorSpace cs = renderer.getResources().getColorSpace(COSName.DEVICEGRAY);
                //                        getGraphicsState().setNonStrokingColorSpace(cs);

                //                        setNonStrokingColor(operator, operands);
                //                        break;
                //                    }
                //                case "gs":
                //                    setGraphicsStateParameters(operator, operands);
                //                    break;
                case "i":
                    {
                        float flatness = (float)operation.operands[0];
                        getCurrentState().flatnessTolerance = flatness;
                    }
                    break;
                //            case "j":
                //                if (operands.size() < 1)
                //                {
                //                    throw new MissingOperandException(operator, operands);
                //    }

                //    int lineJoinStyle = ((COSNumber)operands.get(0)).intValue();
                //    getGraphicsState().setLineJoin(lineJoinStyle);
                //                break;
                //            case "J":
                //                if (operands.size() < 1)
                //                {
                //                    throw new MissingOperandException(operator, operands);
                //}

                //int lineCapStyle = ((COSNumber)operands.get(0)).intValue();
                //getGraphicsState().setLineCap(lineCapStyle );
                //                break;
                //            case "K":
                //            {
                //                PDColorSpace cs = renderer.getResources().getColorSpace(COSName.DEVICECMYK);
                //getGraphicsState().setStrokingColorSpace(cs);

                //setStrokingColor(operator, operands);
                //                break;
                //            }
                //            case "k":
                //            {
                //                PDColorSpace cs = renderer.getResources().getColorSpace(COSName.DEVICECMYK);
                //getGraphicsState().setNonStrokingColorSpace(cs);

                //setNonStrokingColor(operator, operands);
                //                break;
                //            }
                //            case "M":
                //                if (operands.size() < 1)
                //                {
                //                    throw new MissingOperandException(operator, operands);
                //                }

                //                COSNumber miterLimit = (COSNumber)operands.get(0);
                //getGraphicsState().setMiterLimit(miterLimit.floatValue());
                //                break;
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
                case "rg":
                    {
                        List<float> floats = operation.getOperandsAsFloats();
                        Color color = Color.FromArgb(
                            (int)(255 * floats[0]),
                            (int)(255 * floats[1]),
                            (int)(255 * floats[2]));

                        getCurrentState().nonStrokingColor = color;
                    }
                    break;
                case "RG":
                    {
                        List<float> floats = operation.getOperandsAsFloats();
                        Color color = Color.FromArgb(
                            (int)(255 * floats[0]), 
                            (int)(255 * floats[1]), 
                            (int)(255 * floats[2]));

                        getCurrentState().strokingColor = color;
                    }
                    break;
                //            case "ri":
                //            {
                //                if (operands.size() < 1)
                //                {
                //                    throw new MissingOperandException(operator, operands);
                //                }

                //                COSBase base = operands.get(0);
                //                if (!(base instanceof COSName))
                //                {
                //                    return;
                //                }

                //                getGraphicsState().setRenderingIntent(RenderingIntent.fromString(((COSName)base).getName()));
                //                break;
                //            }
                //            case "SC":
                //                setStrokingColor(operator, operands);
                //                break;
                //            case "sc":
                //                setNonStrokingColor(operator, operands);
                //                break;
                //            case "SCN":
                //                setStrokingColor(operator, operands);
                //                break;
                //            case "scn":
                //                setNonStrokingColor(operator, operands);
                //                break;
                case "w":
                    float lineWidth = operation.operands[0] is float ? (float)operation.operands[0] : (int)operation.operands[0];
                    getCurrentState().lineWidth = lineWidth;
                    break;
                    //            }
            }
        }
    }
}
