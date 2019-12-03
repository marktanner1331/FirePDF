using FirePDF.Model;
using FirePDF.Reading;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FirePDF.Processors
{
    public interface IStreamProcessor
    {
        void willStartReadingPage(RecursiveStreamReader parser);

        /// <summary>
        /// called when a new stream has started being read
        /// the stream will have been added to the top of the stream stack in the RecursiveStreamParser
        /// </summary>
        void didStartReadingStream(IStreamOwner streamOwner);

        void processOperation(Operation operation);

        void willFinishReadingStream();

        void willFinishReadingPage();
    }
}
