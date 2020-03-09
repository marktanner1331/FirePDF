using System;
using System.Collections.Generic;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FirePDF.Model;
using FirePDF.Rendering;
using GraphicsState = FirePDF.Model.GraphicsState;

namespace FirePDF.Processors
{
    public class TextProcessor
    {
        private readonly IRenderer renderer;

        private bool isInTextObject = false;
        private readonly Func<GraphicsState> getGraphicsState;
        private Func<PDFResources> getResources;

        public TextProcessor(Func<Model.GraphicsState> getGraphicsState, Func<PDFResources> getResources, IRenderer renderer)
        {
            this.getGraphicsState = getGraphicsState;
            this.getResources = getResources;
            this.renderer = renderer;
        }

        public bool processOperation(Operation operation)
        {
            switch (operation.operatorName)
            {
                case "BT":
                    {
                        isInTextObject = true;

                        GraphicsState g = getGraphicsState();
                        g.textMatrix = new Matrix();
                        g.textLineMatrix = new Matrix();
                        break;
                    }
                case "ET":
                    isInTextObject = false;
                    break;
                case "Tc":
                    getGraphicsState().characterSpacing = operation.getOperandAsFloat(0);
                    break;
                case "Td":
                    {
                        List<float> operands = operation.getOperandsAsFloats();
                        Matrix temp = new Matrix(1, 0, 0, 1, operands[0], operands[1]);

                        GraphicsState g = getGraphicsState();
                        g.textLineMatrix.Multiply(temp);
                        g.textMatrix = g.textLineMatrix.Clone();
                        break;
                    }
                case "TD":
                    {
                        getGraphicsState().textLeading = -operation.getOperandAsFloat(1);
                        goto case "Td";
                    }
                case "Tf":
                    getGraphicsState().font = getResources().getFont(operation.getOperandAsName(0));
                    if(operation.getOperandAsName(0) == "R16")
                    {

                    }
                    getGraphicsState().fontSize = operation.getOperandAsFloat(1);
                    break;
                case "Tj":
                    //TODO: im sure i have to update the text matrix, moving to the right
                    if(operation.operands[0] is byte[] bytes)
                    {
                        renderer.drawText(bytes);
                    }
                    else
                    {
                        renderer.drawText((operation.operands[0] as PDFString).bytes);
                    }
                    break;
                case "TJ":
                    {
                        GraphicsState g = getGraphicsState();

                        foreach(object operand in (PDFList)operation.operands[0])
                        {
                            if(operand is PDFString pdfString)
                            {
                                renderer.drawText(pdfString.bytes);
                            }
                            else if(operand is float || operand is int)
                            {
                                //TODO i really don't think the below is right
                                //aparently a positive adjustment should move it left?
                                Matrix temp = new Matrix(1, 0, 0, 1, (float)Convert.ToDouble(operand) / 1000, 0);
                                g.textMatrix.Multiply(temp);
                            }
                            else
                            {
                                throw new Exception();
                            }
                        }
                    }
                    break;
                case "TL":
                    getGraphicsState().textLeading = operation.getOperandAsFloat(0);
                    break;
                case "Tm":
                    {
                        List<float> operands = operation.getOperandsAsFloats();
                        Matrix temp = new Matrix(operands[0], operands[1], operands[2], operands[3], operands[4], operands[5]);

                        GraphicsState g = getGraphicsState();
                        g.textMatrix = temp;
                        g.textLineMatrix = temp.Clone();
                        break;
                    }
                case "Tr":
                    getGraphicsState().textRenderingMode = (TextRenderingMode)operation.getOperandAsInt(0);
                    break;
                case "Ts":
                    getGraphicsState().textRise = operation.getOperandAsFloat(0);
                    break;
                case "Tw":
                    getGraphicsState().wordSpacing = operation.getOperandAsFloat(0);
                    break;
                case "Tz":
                    getGraphicsState().horizontalScaling = operation.getOperandAsFloat(0) / 100;
                    break;
                case "T*":
                    {
                        GraphicsState g = getGraphicsState();

                        Matrix temp = new Matrix(1, 0, 0, 1, 0, -g.textLeading);

                        g.textLineMatrix.Multiply(temp);
                        g.textMatrix = g.textLineMatrix.Clone();
                        break;
                    }
                case "'":
                    processOperation(new Operation("T*", null));
                    processOperation(new Operation("Tj", operation.operands));
                    break;
                case "\"":
                    getGraphicsState().wordSpacing = operation.getOperandAsFloat(0);
                    getGraphicsState().characterSpacing = operation.getOperandAsFloat(1);
                    processOperation(new Operation("Tj", operation.operands.Skip(2).ToList()));
                    break;
                default:
                    return false;
            }

            return true;
        }
    }
}
