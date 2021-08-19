using FirePDF;
using FirePDF.Model;
using FirePDF.Processors;
using FirePDF.Reading;
using FirePDF.Writing;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace Samples
{
    class CombineStreams
    {
        class CombineStreamsInner : IStreamProcessor
        {
            //TODO might have to check for the default color spaces in form XObjects and merge those

            private Page root;
            RecursiveStreamReader parser;

            private Stream newStream;
            private ContentStreamWriter writer;

            public CombineStreamsInner(Page root)
            {
                this.root = root;
            }

            public void DidStartReadingStream(IStreamOwner streamOwner)
            {
                if (parser.Stream == root)
                {
                    return;
                }

                //writer.writeOperation(new Operation("q", new List<object>()));

                //wish we had mixins in c#
                PdfDictionary dict = ((HaveUnderlyingDict)streamOwner).UnderlyingDict;

                //if(dict.ContainsKey("Matrix"))
                //{
                //    List<Object> matrix = dict.Get<PdfList>("Matrix").Cast<object>().ToList();
                //    writer.writeOperation(new Operation("cm", matrix));
                //}

                //List<Object> BBox = dict.Get<PdfList>("BBox").Cast<object>().ToList();
                //writer.writeOperation(new Operation("re", BBox));

                //writer.writeOperation(new Operation("W", new List<object>()));
                //writer.writeOperation(new Operation("n", new List<object>()));

                //and i don't know what Groups do
            }

            public void ProcessOperation(Operation operation)
            {
                //if we are processing the root then we don't have to move any resources
                if (parser.Stream == root)
                {
                    writer.writeOperation(operation);
                    return;
                }

                //load the existing resource

                Name resourceName;
                string resourcePrefix;

                switch (operation.operatorName)
                {
                    case "Do":
                        resourceName = operation.GetOperandAsName(0);
                        resourcePrefix = "XObject";
                        break;
                    case "gs":
                        resourceName = operation.GetOperandAsName(0);
                        resourcePrefix = "ExtGState";
                        break;
                    case "Tf":
                        resourceName = operation.GetOperandAsName(0);
                        resourcePrefix = "Font";
                        break;
                    case "DP":
                    case "BDC":
                        if (operation.operands[1] is Name == false)
                        {
                            goto default;
                        }

                        resourceName = operation.GetOperandAsName(1);
                        resourcePrefix = "Properties";
                        break;
                    case "CS":
                    case "cs":
                        resourceName = operation.GetOperandAsName(0);

                        //skip over named color spaces
                        switch (resourceName)
                        {
                            case "DeviceGray":
                            case "DeviceRGB":
                            case "DeviceCYMK":
                            case "Pattern":
                                writer.writeOperation(operation);
                                return;
                        }

                        resourcePrefix = "ColorSpace";
                        break;
                    case "SCN":
                    case "scn":
                        if (operation.operands.Last() is Name == false)
                        {
                            goto default;
                        }

                        resourceName = operation.operands.Last() as Name;
                        resourcePrefix = "Pattern";
                        break;
                    default:
                        writer.writeOperation(operation);
                        return;
                }

                //copy it to the root

                object resource = parser.Resources.GetUnresolvedObjectAtPath(resourcePrefix, resourceName);
                if (resource == null)
                {

                }

                Name newName = root.Resources.generateNewResourceName(resourcePrefix);
                root.Resources.SetObjectAtPath(resource, resourcePrefix, newName);

                //and update the operation with the new resource name

                switch (operation.operatorName)
                {
                    case "gs":
                        operation.operands[0] = newName;
                        break;
                    case "Tf":
                        operation.operands[0] = newName;
                        break;
                    case "DP":
                    case "BDC":
                        operation.operands[1] = newName;
                        break;
                    case "CS":
                    case "cs":
                        operation.operands[0] = newName;
                        break;
                    case "SCN":
                    case "scn":
                        operation.operands[operation.operands.Count - 1] = newName;
                        break;
                    case "Do":
                        operation.operands[0] = newName;
                        break;
                    default:
                        throw new Exception("Really shouldn't happen");
                }

                writer.writeOperation(operation);
            }

            public void WillFinishReadingPage()
            {
                writer.flush();
                newStream.Seek(0, SeekOrigin.Begin);
                ObjectReference streamRef = root.Pdf.AddStream(newStream);

                root.UnderlyingDict.Set("Contents", streamRef);

                foreach (Name form in root.Resources.ListFormXObjectNames().ToList())
                {
                    root.Resources.removeXObject(form);
                }
            }

            public void WillFinishReadingStream()
            {
                //writer.writeOperation(new Operation("Q", new List<object>()));
            }

            public void WillStartReadingPage(RecursiveStreamReader parser)
            {
                this.parser = parser;
                newStream = new MemoryStream();
                writer = new ContentStreamWriter(newStream);
            }
        }
    }
}
