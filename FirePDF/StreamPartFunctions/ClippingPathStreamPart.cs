using FirePDF.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FirePDF.StreamPartFunctions
{
    class ClippingPathStreamPart : StreamPart
    {
        public ClippingPathStreamPart(List<Operation> operations) : base(operations)
        {
            variables["type"] = "clippingPath";
        }
    }
}
