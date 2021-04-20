using FirePDF.Model;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace FirePDF.Modifying
{
    public interface IStreamModifier
    {
        void WillStartReadingRoot(RecursiveStreamRewriter recursiveStreamRewriter);
        void WillFinishReadingRoot();
        void DidStartReadingStream(IStreamOwner streamOwner, Stream stream);
        void WillFinishReadingStream();
        void ProcessOperation(Operation operation);
    }
}
