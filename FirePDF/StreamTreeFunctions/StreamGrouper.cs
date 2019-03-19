using FirePDF.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FirePDF.StreamTreeFunctions
{
    public static class StreamGrouper
    {
        public static List<StreamPart> groupStreamIntoParts(IEnumerable<Operation> operations)
        {
            throw new Exception();
            foreach(Operation operation in operations)
            {
                switch(operation.operatorName)
                {
                    case "m":

                        break;
                }
            }
        }
    }
}
