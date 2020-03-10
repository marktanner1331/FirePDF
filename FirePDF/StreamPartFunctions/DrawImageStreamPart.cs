using FirePDF.Model;
using System;
using System.Collections.Generic;
using System.Linq;

namespace FirePDF.StreamPartFunctions
{
    public class DrawImageStreamPart : StreamPart
    {
        public DrawImageStreamPart(List<Operation> operations) : base(operations)
        {
            variables["type"] = "drawImage";
        }

        /// <summary>
        /// increases the width and height of the image by 1 pixel
        /// </summary>
        public void IncreaseSizeByOnePixel()
        {
            Operation cm = operations.First();
            if(cm.operatorName != "cm")
            {
                throw new Exception("expected cm");
            }

            cm.operands[0] = cm.GetOperandAsFloat(0);
            cm.operands[3] = cm.GetOperandAsFloat(3);
        }
    }
}
