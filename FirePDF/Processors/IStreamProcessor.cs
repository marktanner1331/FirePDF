using FirePDF.Model;
using FirePDF.Reading;

namespace FirePDF.Processors
{
    public interface IStreamProcessor
    {
        void WillStartReadingPage(RecursiveStreamReader parser);

        /// <summary>
        /// called when a new stream has started being read
        /// the stream will have been added to the top of the stream stack in the RecursiveStreamParser
        /// </summary>
        void DidStartReadingStream(IStreamOwner streamOwner);

        void ProcessOperation(Operation operation);

        void WillFinishReadingStream();

        void WillFinishReadingPage();
    }
}
