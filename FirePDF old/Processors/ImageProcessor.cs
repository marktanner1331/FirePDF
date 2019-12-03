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
        private Func<PDFResources> getResources;
        private IRenderer renderer;

        public ImageProcessor(Func<PDFResources> getResources, IRenderer renderer)
        {
            this.getResources = getResources;
            this.renderer = renderer;
        }

        public bool processOperation(Operation operation)
        {
            switch (operation.operatorName)
            {
                case "Do":
                    string xObjectName = (Name)operation.operands[0];
                    object xObject = getResources().getObjectAtPath("XObject", xObjectName);

                    if(xObject is XObjectImage)
                    {
                        renderer?.drawImage(xObject as XObjectImage);
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
