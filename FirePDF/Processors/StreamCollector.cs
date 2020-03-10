using System.Collections.Generic;
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

        public void DidStartReadingStream(IStreamOwner streamOwner)
        {
            
        }

        public void ProcessOperation(Operation operation)
        {
            operations.Add(operation);
        }

        public void WillFinishReadingPage()
        {
            
        }

        public void WillFinishReadingStream()
        {
            
        }

        public void WillStartReadingPage(RecursiveStreamReader parser)
        {
            operations.Clear();
        }
    }
}
