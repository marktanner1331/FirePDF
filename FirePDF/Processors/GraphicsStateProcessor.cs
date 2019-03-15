using FirePDF.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Matrix = System.Drawing.Drawing2D.Matrix;

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
            switch (operation.operatorName)
            {
                case "cm":
                    {
                        List<float> operands = operation.getOperationsAsFloats();

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
                //                case "i":
                //                    if (operands.size() < 1)
                //                    {
                //                        throw new MissingOperandException(operator, operands);
                //                    }

                //                    if (!checkArrayTypesClass(operands, COSNumber.class))
                //                {
                //                    return;
                //                }

                //                COSNumber value = (COSNumber)operands.get(0);
                //        getGraphicsState().setFlatness(value.floatValue());
                //                break;
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
                    //            case "RG":
                    //            {
                    //                PDColorSpace cs = renderer.getResources().getColorSpace(COSName.DEVICERGB);
                    //getGraphicsState().setStrokingColorSpace(cs);

                    //setStrokingColor(operator, operands);
                    //                break;
                    //            }
                    //            case "rg":
                    //            {
                    //                PDColorSpace cs = renderer.getResources().getColorSpace(COSName.DEVICERGB);
                    //getGraphicsState().setNonStrokingColorSpace(cs);

                    //setNonStrokingColor(operator, operands);
                    //                break;
                    //            }
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
                    //            case "w":
                    //                if (operands.size() < 1)
                    //                {
                    //                    throw new MissingOperandException(operator, operands);
                    //                }

                    //                COSNumber width = (COSNumber)operands.get(0);
                    //getGraphicsState().setLineWidth(width.floatValue());
                    //                break;
                    //            }
            }
        }
    }
}
