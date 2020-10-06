using System;
using System.Collections.Generic;
using System.Drawing.Drawing2D;
using System.Linq;
using FirePDF.Model;
using FirePDF.Rendering;
using GraphicsState = FirePDF.Model.GraphicsState;

namespace FirePDF.Text
{
    public class TextProcessor
    {
        private readonly Renderer renderer;

        private bool isInTextObject;
        private readonly Func<GraphicsState> getGraphicsState;
        private readonly Func<PdfResources> getResources;

        public TextProcessor(Func<GraphicsState> getGraphicsState, Func<PdfResources> getResources, Renderer renderer)
        {
            this.getGraphicsState = getGraphicsState;
            this.getResources = getResources;
            this.renderer = renderer;
        }

        public static bool isTextOperation(Operation operation)
        {
            switch (operation.operatorName)
            {
                case "BT":
                case "ET":
                case "Tc":
                case "Td":
                case "TD":
                case "Tf":
                case "Tj":
                case "TJ":
                case "TL":
                case "Tm":
                case "Tr":
                case "Ts":
                case "Tw":
                case "Tz":
                case "T*":
                case "'":
                case "\"":
                    return true;
                default:
                    return false;
            }
        }

        public bool ProcessOperation(Operation operation)
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
                    getGraphicsState().characterSpacing = operation.GetOperandAsFloat(0);
                    break;
                case "Td":
                    {
                        List<float> operands = operation.GetOperandsAsFloats();
                        Matrix temp = new Matrix(1, 0, 0, 1, operands[0], operands[1]);

                        GraphicsState g = getGraphicsState();
                        g.textLineMatrix.Multiply(temp);
                        g.textMatrix = g.textLineMatrix.Clone();
                        break;
                    }
                case "TD":
                    {
                        getGraphicsState().textLeading = -operation.GetOperandAsFloat(1);
                        goto case "Td";
                    }
                case "Tf":
                    getGraphicsState().font = getResources().GetFont(operation.GetOperandAsName(0));
                    getGraphicsState().fontSize = operation.GetOperandAsFloat(1);
                    break;
                case "Tj":
                    //TODO: im sure i have to update the text matrix, moving to the right
                    if(operation.operands[0] is byte[] bytes)
                    {
                        renderer.DrawText(bytes);
                    }
                    else
                    {
                        renderer.DrawText((operation.operands[0] as PdfString).ToByteArray());
                    }
                    break;
                case "TJ":
                    {
                        GraphicsState g = getGraphicsState();

                        foreach(object operand in (PdfList)operation.operands[0])
                        {
                            if(operand is PdfString pdfString)
                            {
                                renderer.DrawText(pdfString.ToByteArray());
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
                    getGraphicsState().textLeading = operation.GetOperandAsFloat(0);
                    break;
                case "Tm":
                    {
                        List<float> operands = operation.GetOperandsAsFloats();
                        Matrix temp = new Matrix(operands[0], operands[1], operands[2], operands[3], operands[4], operands[5]);

                        GraphicsState g = getGraphicsState();
                        g.textMatrix = temp;
                        g.textLineMatrix = temp.Clone();
                        break;
                    }
                case "Tr":
                    getGraphicsState().textRenderingMode = (TextRenderingMode)operation.GetOperandAsInt(0);
                    break;
                case "Ts":
                    getGraphicsState().textRise = operation.GetOperandAsFloat(0);
                    break;
                case "Tw":
                    getGraphicsState().wordSpacing = operation.GetOperandAsFloat(0);
                    break;
                case "Tz":
                    getGraphicsState().horizontalScaling = operation.GetOperandAsFloat(0) / 100;
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
                    ProcessOperation(new Operation("T*", null));
                    ProcessOperation(new Operation("Tj", operation.operands));
                    break;
                case "\"":
                    getGraphicsState().wordSpacing = operation.GetOperandAsFloat(0);
                    getGraphicsState().characterSpacing = operation.GetOperandAsFloat(1);
                    ProcessOperation(new Operation("Tj", operation.operands.Skip(2).ToList()));
                    break;
                default:
                    return false;
            }

            return true;
        }
    }
}
