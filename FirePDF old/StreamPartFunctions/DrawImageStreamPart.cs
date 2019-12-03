using FirePDF.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FirePDF.StreamPartFunctions
{
    class DrawImageStreamPart : StreamPart
    {
        public DrawImageStreamPart(List<Operation> operations) : base(operations)
        {
            variables["type"] = "drawImage";
        }

        /// <summary>
        /// increases the width and height of the image by 1 pixel
        /// </summary>
        public void increaseSizeByOnePixel()
        {
            Operation cm = operations.First();
            if(cm.operatorName != "cm")
            {
                throw new Exception("expected cm");
            }

            cm.operands[0] = cm.getOperandAsFloat(0);
            cm.operands[3] = cm.getOperandAsFloat(3);
        }
    }
}
