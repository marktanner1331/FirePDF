using FirePDF;
using FirePDF.Model;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;

namespace Samples
{
    public class GetSoftMasks
    {
        internal static void Main(string[] args)
        {
            const string file = @"C:\Users\Mark\Documents\pagesuite\tickets\CS-1091 - Egmont quality\orig 2.pdf";
            using (Pdf pdf = new Pdf(file))
            {
                HashSet<ObjectReference> maskRefs = new HashSet<ObjectReference>();
                int i = 0;
                Page page = pdf.GetPage(1);
                foreach (IStreamOwner owner in page.listIStreamOwners())
                {
                    foreach (ExtGState state in owner.Resources.GetAllExtGStates())
                    {
                        if (state.HasSoftMask(out SoftMask mask))
                        {
                            ObjectReference maskRef = pdf.ReverseGet(mask);
                            if (maskRefs.Contains(maskRef))
                            {
                                continue;
                            }

                            XObjectForm group = mask.TransparencyGroup;
                            List<ObjectReference> imageRefs = group.Resources.ListImages(false).ToList();
                            if (imageRefs.Count != 1)
                            {
                                continue;
                            }

                            XObjectImage image = imageRefs.First().Get<XObjectImage>();
                            Bitmap b = image.GetImage();
                            b.RotateFlip(RotateFlipType.RotateNoneFlipY);
                            b.Save("./test " + ++i + ".jpg");
                        }
                    }
                }

                //pdf.Save(@"C:\Users\Mark\Documents\pagesuite\tickets\CS-653 - Sun\isolated 18.pdf", SaveType.Fresh);
            }
        }
    }
}
