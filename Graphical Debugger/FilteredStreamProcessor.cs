using FirePDF.Model;
using FirePDF.Processors;
using FirePDF.Rendering;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Graphical_Debugger
{
    internal class FilteredStreamProcessor : StreamProcessor
    {
        private List<bool> operationMap;
        private int i = 0;

        public FilteredStreamProcessor(List<bool> operationMap, IRenderer renderer) : base(renderer)
        {
            this.operationMap = operationMap;
        }

        public override void processOperation(Operation operation)
        {
            if(operationMap[i])
            {
                base.processOperation(operation);
            }

            i++;
        }
    }
}
