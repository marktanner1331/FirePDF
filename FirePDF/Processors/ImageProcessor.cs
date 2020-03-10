using FirePDF.Model;
using FirePDF.Rendering;
using System;

namespace FirePDF.Processors
{
    /// <summary>
    /// responsible for processing images such as BI and Do
    /// </summary>
    public class ImageProcessor
    {
        private readonly Func<PdfResources> getResources;
        private readonly Renderer renderer;

        public ImageProcessor(Func<PdfResources> getResources, Renderer renderer)
        {
            this.getResources = getResources;
            this.renderer = renderer;
        }

        public bool ProcessOperation(Operation operation)
        {
            switch (operation.operatorName)
            {
                case "Do":
                    string xObjectName = (Name)operation.operands[0];
                    object xObject = getResources().GetObjectAtPath("XObject", xObjectName);

                    if(xObject is XObjectImage)
                    {
                        renderer?.DrawImage(xObject as XObjectImage);
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
