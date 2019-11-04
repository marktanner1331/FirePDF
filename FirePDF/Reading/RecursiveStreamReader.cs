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
    public class RecursiveStreamReader : IStreamOwner
    {
        private IStreamProcessor streamProcessor;
        
        private Stack<PDFResources> resourcesStack;
        private Stack<IStreamOwner> streamStack;
        private Stream currentStream;

        private Page page;

        public RecursiveStreamReader(IStreamProcessor streamProcessor)
        {
            this.streamProcessor = streamProcessor;
        }

        public PDFResources resources => resourcesStack.Peek();

        public RectangleF boundingBox => streamStack.Count > 0 ? streamStack.Peek().boundingBox : RectangleF.Empty;

        /// <summary>
        /// returns the stream that is currently being read (or null if we are not in readPage())s
        /// </summary>
        public Stream readContentStream()
        {
            return currentStream;
        }

        public void readPage(Page page)
        {
            this.page = page;

            resourcesStack = new Stack<PDFResources>();
            streamStack = new Stack<IStreamOwner>();

            //we always need resources, so initializing the stack with an empty resourcse object
            //will ensure that everything is ok, even if its never used
            resourcesStack.Push(new PDFResources());

            streamProcessor.willStartReadingPage(this);
            processStream(page);
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

            streamProcessor.didStartReadingStream();

            Stream oldStream = currentStream;
            currentStream = stream.readContentStream();

            List<Operation> operations = ContentStreamReader.readOperationsFromStream(currentStream);
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
