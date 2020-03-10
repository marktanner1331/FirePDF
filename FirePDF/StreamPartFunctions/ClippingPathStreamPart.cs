using FirePDF.Model;
using System.Collections.Generic;

namespace FirePDF.StreamPartFunctions
{
    public class ClippingPathStreamPart : StreamPart
    {
        public ClippingPathStreamPart(List<Operation> operations) : base(operations)
        {
            variables["type"] = "clippingPath";
        }
    }
}
