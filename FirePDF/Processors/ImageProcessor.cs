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

        public void processOperation(Operation operation)
        {
            switch (operation.operatorName)
            {
                case "Do":
                    string xObjectName = (string)operation.operands[0];
                    object xObject = streamOwner.getResources().getObjectAtPath("XObject", xObjectName);

                    XObjectImage xObjectImage = streamOwner.getResources().getXObjectImage(xObjectName);
                    Image image = xObjectImage.getImage();
                    renderer.drawImage(image);

                    break;
            }
        }
    }
}
