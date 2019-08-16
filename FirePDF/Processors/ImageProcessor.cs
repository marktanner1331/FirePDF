using FirePDF.Model;
using FirePDF.Rendering;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FirePDF.Processors
{
    /// <summary>
    /// responsible for processing images such as BI and Do
    /// </summary>
    public class ImageProcessor
    {
        private IStreamOwner streamOwner;
        private IRenderer renderer;

        public ImageProcessor(IStreamOwner streamOwner, IRenderer renderer)
        {
            this.streamOwner = streamOwner;
            this.renderer = renderer;
        }

        public bool processOperation(Operation operation)
        {
            switch (operation.operatorName)
            {
                case "Do":
                    string xObjectName = (Name)operation.operands[0];
                    object xObject = streamOwner.resources.getObjectAtPath("XObject", xObjectName);

                    if(xObject is XObjectImage)
                    {
                        Image image = (xObject as XObjectImage).getImage();
                        renderer?.drawImage(image);
                        return true;
                    }
                    else
                    {
                        return false;
                    }
                default:
                    return false;
            }
        }
    }
}
