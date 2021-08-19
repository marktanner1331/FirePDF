using FirePDF;
using FirePDF.Model;
using System;
using System.Collections.Generic;
using System.Text;

namespace Samples
{
    class FindAllSubMasks
    {
        public static void Main()
        {
            const string file = @"";
            using (Pdf pdf = new Pdf(file))
            {
                HashSet<ObjectReference> maskRefs = new HashSet<ObjectReference>();

                Page page = pdf.GetPage(1);
                foreach (IStreamOwner owner in page.listIStreamOwners())
                {
                    foreach (ExtGState state in owner.Resources.GetAllExtGStates())
                    {
                        if (state.HasSoftMask(out SoftMask mask))
                        {
                            ObjectReference maskRef = pdf.ReverseGet(mask);
                            maskRefs.Add(maskRef);
                        }
                    }
                }
            }
        }
    }
}
