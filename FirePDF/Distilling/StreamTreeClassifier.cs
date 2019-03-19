using FirePDF.Model;
using FirePDF.StreamTreeFunctions;
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
            foreach(StreamPart part in streamTree.getAllLeafNodes())
            {
                classifyStreamPart(streamOwner, part);
            }
        }

        /// <summary>
        /// sets the containsText, containsImages, containsGraphics, containsClippingPath, and hasBeenClassified tags for the given streamPart
        /// </summary>
        public static void classifyStreamPart(IStreamOwner streamOwner, StreamPart streamPart)
        {
            streamPart.removeTags("containsText", "containsImages", "containsGraphics", "containsClippingPath");
            streamPart.addTag("hasBeenClassified");

            foreach (Operation operation in streamPart.operations)
            {
                switch (operation.operatorName)
                {
                    case "Do":
                        if (streamPart.hasTag("containsImages"))
                        {
                            continue;
                        }

                        if (streamOwner.resources.isXObjectImage((string)operation.operands[0]))
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
                        if((int)operation.operands[0] > 3)
                        {
                            streamPart.addTag("containsClippingPath");
                        }
                        break;
                }
            }
        }
    }
}
