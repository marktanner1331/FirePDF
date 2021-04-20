using FirePDF.Model;
using FirePDF.Reading;
using FirePDF.Writing;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;

namespace FirePDF.Modifying
{
    public class RecursiveStreamRewriter
    {
        private readonly IStreamModifier streamProcessor;

        private Stack<PdfResources> resourcesStack;

        private Stack<List<Operation>> existingOperations;
        private Stack<List<Operation>> newOperations;
        
        //hint: this will be the same streamOwner found at the bottom of the stream stack
        //we cache it here for efficiency
        private IStreamOwner rootStream;

        public RectangleF BoundingBox => rootStream?.BoundingBox ?? RectangleF.Empty;

        public RecursiveStreamRewriter(IStreamModifier streamModifier)
        {
            this.streamProcessor = streamModifier;
        }

        public PdfResources Resources => resourcesStack.Peek();

        public Pdf Pdf => rootStream.Pdf;

        public void ReadStreamRecursively(IStreamOwner stream)
        {
            rootStream = stream;

            resourcesStack = new Stack<PdfResources>();
            existingOperations = new Stack<List<Operation>>();
            newOperations = new Stack<List<Operation>>();
            
            //we always need resources, so initializing the stack with an empty resources object
            //will ensure that everything is ok, even if its never used
            resourcesStack.Push(new PdfResources(new PdfDictionary(Pdf, new Dictionary<Name, object>())));

            streamProcessor.WillStartReadingRoot(this);
            ProcessStreamOwner(stream);
            streamProcessor.WillFinishReadingRoot();
        }

        private void ProcessStreamOwner(IStreamOwner streamOwner)
        {
            PdfResources resources = streamOwner.Resources;
            if (resources == null)
            {
                //if the stream doesnt have its own resources then use the parent resources
                //this is not in the Pdf spec, but the file from
                //PDFBOX-1359 does this and works in Acrobat
                resources = resourcesStack.Peek();
            }

            resourcesStack.Push(resources);
            
            //TODO: maybe support IMultiStreamOwner
            if(streamOwner is Page page)
            {
                foreach(ObjectReference pdfStreamRef in page.ListContentStreams(false))
                {
                    existingOperations.Push(new List<Operation>());
                    newOperations.Push(new List<Operation>());

                    PdfStream pdfStream = pdfStreamRef.Get<PdfStream>();
                    Stream stream = pdfStream.GetDecompressedStream();
                    
                    processStream(streamOwner, stream);
                    stream.Dispose();

                    if(Enumerable.SequenceEqual(existingOperations.Peek(), newOperations.Peek()) == false)
                    {
                        updateStream(pdfStream, newOperations.Peek());
                    }

                    existingOperations.Pop();
                    newOperations.Pop();
                }
            }
            else
            {
                existingOperations.Push(new List<Operation>());
                newOperations.Push(new List<Operation>());

                Stream stream = streamOwner.GetStream();
                processStream(streamOwner, stream);
                stream.Dispose();

                if (Enumerable.SequenceEqual(existingOperations.Peek(), newOperations.Peek()) == false)
                {
                    //XObjectForms inherit directly from PdfStream
                    updateStream((PdfStream)streamOwner, newOperations.Peek());
                }

                existingOperations.Pop();
                newOperations.Pop();
            }

            resourcesStack.Pop();
        }

        private void updateStream(PdfStream stream, List<Operation> operations)
        {
            //gotta keep this stream open until we hit save
            MemoryStream ms = new MemoryStream();

            ContentStreamWriter.WriteOperationsToStream(ms, operations);
            ms.Seek(0, SeekOrigin.Begin);

            stream.UpdateStream(ms);
        }

        private void processStream(IStreamOwner streamOwner, Stream stream)
        {
            streamProcessor.DidStartReadingStream(streamOwner, stream);
            
            List<Operation> operations = ContentStreamReader.ReadOperationsFromStream(Pdf, stream);
            foreach (Operation operation in operations)
            {
                if (operation.operatorName == "Do" && Resources.IsXObjectForm((Name)operation.operands[0]))
                {
                    ProcessStreamOwner(Resources.GetXObjectForm((Name)operation.operands[0]));
                }
                else
                {
                    existingOperations.Peek().Add(operation);
                    streamProcessor.ProcessOperation(operation);
                }
            }

            streamProcessor.WillFinishReadingStream();
        }

        public void AddOperation(Operation operation)
        {
            newOperations.Peek().Add(operation);
        }
    }
}
