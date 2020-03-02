using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FirePDF.Model;
using FirePDF.Reading;

namespace FirePDF.Processors
{
    /// <summary>
    /// a stream processor that does nothing but collects the operations passed to it and provides them as a list
    /// </summary>
    public class StreamCollector : IStreamProcessor
    {
        public readonly List<Operation> operations;

        public StreamCollector()
        {
            operations = new List<Operation>();
        }

        public void didStartReadingStream(IStreamOwner streamOwner)
        {
            
        }

        public void processOperation(Operation operation)
        {
            operations.Add(operation);
        }

        public void willFinishReadingPage()
        {
            
        }

        public void willFinishReadingStream()
        {
            
        }

        public void willStartReadingPage(RecursiveStreamReader parser)
        {
            operations.Clear();
        }
    }
}
