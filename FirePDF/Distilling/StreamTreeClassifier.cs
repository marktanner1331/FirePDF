using FirePDF.Model;
using FirePDF.StreamPartFunctions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FirePDF.Distilling
{
    public static class StreamTreeClassifier
    {
        public static void classifyStreamTree(IStreamOwner streamOwner, StreamTree streamTree)
        {
            streamTree.swapParts(x => classifyAndSwapStreamPart(streamOwner, x));
        }

        /// <summary>
        /// detects the type of stream part passed in and if detection is successful then a new stream part is returned
        /// which represents the operations better
        /// </summary>
        private static StreamPart classifyAndSwapStreamPart(IStreamOwner streamOwner, StreamPart streamPart)
        {
            if (Classifier.isClippingPath(streamPart.operations))
            {
                return new ClippingPathStreamPart(streamPart.operations);
            }
            else if (Classifier.isDrawImage(streamPart.operations))
            {
                return new DrawImageStreamPart(streamPart.operations);
            }
            else
            {
                classifyStreamPartAsMixed(streamOwner, streamPart);
                return streamPart;
            }
        }

        public static void classifyStreamPart(IStreamOwner streamOwner, StreamPart streamPart)
        {
            if(Classifier.isClippingPath(streamPart.operations))
            {
                streamPart.variables["type"] = "clippingPath";
            }
            else if(Classifier.isDrawImage(streamPart.operations))
            {
                streamPart.variables["type"] = "image";
            }
            else
            {
                classifyStreamPartAsMixed(streamOwner, streamPart);
            }
        }

        /// <summary>
        /// sets the containsText, containsImages, containsGraphics, containsClippingPath, and hasBeenClassified tags for the given streamPart
        /// </summary>
        public static void classifyStreamPartAsMixed(IStreamOwner streamOwner, StreamPart streamPart)
        {
            streamPart.removeTags("containsText", "containsImages", "containsGraphics", "containsClippingPath");

            streamPart.addTag("hasBeenClassified");
            streamPart.variables["type"] = "mixed";

            foreach (Operation operation in streamPart.operations)
            {
                switch (operation.operatorName)
                {
                    case "Do":
                        if (streamPart.hasTag("containsImages"))
                        {
                            continue;
                        }

                        if (streamOwner.resources.isXObjectImage((Name)operation.operands[0]))
                        {
                            streamPart.addTag("containsImages");
                        }
                        break;
                    case "TJ":
                    case "Tj":
                        streamPart.addTag("containsText");
                        break;
                    case "sh":
                    case "S":
                    case "s":
                    case "F":
                    case "f":
                    case "f*":
                    case "B":
                    case "B*":
                    case "b":
                    case "sc":
                    case "SCN":
                    case "b*":
                        streamPart.addTag("containsGraphics");
                        break;
                    case "W":
                    case "W*":
                        streamPart.addTag("containsClippingPath");
                        break;
                    case "Tr":
                        if ((int)operation.operands[0] > 3)
                        {
                            streamPart.addTag("containsClippingPath");
                        }
                        break;
                }
            }
        }


    }
}
