using FirePDF.Model;
using FirePDF.Processors;
using FirePDF.Rendering;
using System.Collections.Generic;

namespace Graphical_Debugger
{
    internal class FilteredStreamProcessor : StreamProcessor
    {
        private readonly bool[] operationMap;
        private int i = 0;

        public FilteredStreamProcessor(List<bool> operationMap, Renderer renderer) : base(renderer)
        {
            this.operationMap = operationMap.ToArray();
        }

        public override void ProcessOperation(Operation operation)
        {
            if(operationMap[i])
            {
                base.ProcessOperation(operation);
            }

            i++;
        }
    }
}
