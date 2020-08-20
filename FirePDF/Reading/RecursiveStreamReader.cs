using FirePDF.Model;
using FirePDF.Processors;
using System.Collections.Generic;
using System.Drawing;
using System.IO;

namespace FirePDF.Reading
{
    public class RecursiveStreamReader
    {
        private readonly IStreamProcessor streamProcessor;
        
        private Stack<PdfResources> resourcesStack;
        private Stack<IStreamOwner> streamStack;
        private Stream currentStream;

        //hint: this will be the same streamOwner found at the bottom of the stream stack
        //we cache it here for efficiency
        private IStreamOwner rootStream;

        public RectangleF BoundingBox => rootStream?.BoundingBox ?? RectangleF.Empty;

        public RecursiveStreamReader(IStreamProcessor streamProcessor)
        {
            this.streamProcessor = streamProcessor;
        }

        public PdfResources Resources => resourcesStack.Peek();
        
        public Pdf Pdf => rootStream.Pdf;
        
        public void ReadStreamRecursively(IStreamOwner stream)
        {
            rootStream = stream;

            resourcesStack = new Stack<PdfResources>();
            streamStack = new Stack<IStreamOwner>();

            //we always need resources, so initializing the stack with an empty resources object
            //will ensure that everything is ok, even if its never used
            resourcesStack.Push(new PdfResources(new PdfDictionary(Pdf, new Dictionary<Name, object>())));

            streamProcessor.WillStartReadingPage(this);
            ProcessStream(stream);
            streamProcessor.WillFinishReadingPage();
        }

        private void ProcessStream(IStreamOwner stream)
        {
            PdfResources resources = stream.Resources;
            if (resources == null)
            {
                //if the stream doesnt have its own resources then use the parent resources
                //this is not in the Pdf spec, but the file from
                //PDFBOX-1359 does this and works in Acrobat
                resources = resourcesStack.Peek();
            }

            resourcesStack.Push(resources);
            streamStack.Push(stream);

            streamProcessor.DidStartReadingStream(stream);

            Stream oldStream = currentStream;
            currentStream = stream.GetStream();

            List<Operation> operations = ContentStreamReader.ReadOperationsFromStream(Pdf, currentStream);
            foreach(Operation operation in operations)
            {
                if(operation.operatorName == "Do" && resources.IsXObjectForm((Name)operation.operands[0]))
                {
                    ProcessStream(resources.GetXObjectForm((Name)operation.operands[0]));
                }
                else
                {
                    streamProcessor.ProcessOperation(operation);
                }
            }

            streamProcessor.WillFinishReadingStream();

            currentStream.Dispose();
            currentStream = oldStream;

            streamStack.Pop();
            resourcesStack.Pop();
        }
    }
}
