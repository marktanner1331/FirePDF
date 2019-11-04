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
    class TextProcessor
    {
        private readonly IRenderer renderer;

        private bool isInTextObject = false;
        private readonly Func<Model.GraphicsState> getGraphicsState;

        public TextProcessor(Func<Model.GraphicsState> getGraphicsState, IStreamOwner streamOwner, IRenderer renderer)
        {
            this.renderer = renderer;

            this.getGraphicsState = getGraphicsState;
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
                        g.textMatrix.Invert();
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
                        throw new Exception("should work but needs testing");
                        getGraphicsState().textLeading = -operation.getOperandAsFloat(1);
                        goto case "Td";
                    }
                case "Tf":
                    getGraphicsState().fontSize = operation.getOperandAsFloat(1);
                    break;
                case "Tj":

                    break;
                case "TJ":

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
