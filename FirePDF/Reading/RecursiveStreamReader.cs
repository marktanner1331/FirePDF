using FirePDF.Model;
using FirePDF.Processors;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FirePDF.Reading
{
    public class RecursiveStreamReader
    {
        private IStreamProcessor streamProcessor;
        
        private Stack<PDFResources> resourcesStack;
        private Stack<IStreamOwner> streamStack;
        private Stream currentStream;

        //hint: this will be the same streamOwner found at the bottom of the stream stack
        //we cache it here for efficiency
        private IStreamOwner rootStream;

        public RectangleF boundingBox => rootStream?.boundingBox ?? RectangleF.Empty;

        public RecursiveStreamReader(IStreamProcessor streamProcessor)
        {
            this.streamProcessor = streamProcessor;
        }

        public PDFResources resources => resourcesStack.Peek();
        
        public PDF pdf => rootStream.pdf;
        
        public void readStreamRecursively(IStreamOwner stream)
        {
            this.rootStream = stream;

            resourcesStack = new Stack<PDFResources>();
            streamStack = new Stack<IStreamOwner>();

            //we always need resources, so initializing the stack with an empty resources object
            //will ensure that everything is ok, even if its never used
            resourcesStack.Push(new PDFResources(rootStream, new PDFDictionary(pdf, new Dictionary<Name, object>())));

            streamProcessor.willStartReadingPage(this);
            processStream(stream);
            streamProcessor.willFinishReadingPage();
        }

        private void processStream(IStreamOwner stream)
        {
            PDFResources resources = stream.resources;
            if (resources == null)
            {
                //if the stream doesnt have its own resources then use the parent resources
                //this is not in the PDF spec, but the file from
                //PDFBOX-1359 does this and works in Acrobat
                resources = resourcesStack.Peek();
            }

            resourcesStack.Push(resources);
            streamStack.Push(stream);

            streamProcessor.didStartReadingStream(stream);

            Stream oldStream = currentStream;
            currentStream = stream.getStream();

            List<Operation> operations = ContentStreamReader.readOperationsFromStream(pdf, currentStream);
            foreach(Operation operation in operations)
            {
                if(operation.operatorName == "Do" && resources.isXObjectForm((Name)operation.operands[0]))
                {
                    processStream(resources.getXObjectForm((Name)operation.operands[0]));
                }
                else
                {
                    streamProcessor.processOperation(operation);
                }
            }

            streamProcessor.willFinishReadingStream();

            currentStream.Dispose();
            currentStream = oldStream;

            streamStack.Pop();
            resourcesStack.Pop();
        }
    }
}
