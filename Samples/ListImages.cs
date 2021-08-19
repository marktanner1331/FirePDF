using FirePDF;
using FirePDF.Model;
using FirePDF.Processors;
using FirePDF.Reading;
using ObjectDumper;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace Samples
{
    class ListImages
    {
        class Temp
        {
            public string FileName;
            public double minDPI;
            public double maxDPI;
            public double averageDPI;
        }

        internal static void Main1(string[] args)
        {
            const string file = @"C:\Users\Mark\Documents\pagesuite\tickets\CS-1091 - Egmont quality\page.pdf";

            using (Pdf pdf = new Pdf(file))
            {
                Page page = pdf.GetPage(1);

                CustomImageProcessor sp = new CustomImageProcessor();

                RecursiveStreamReader streamReader = new RecursiveStreamReader(sp);
                streamReader.ReadStreamRecursively(page);

                sp.images.ForEach(x => x.Dump(x.ObjectNumber.ToString()));
            }
        }

        internal static void Main(string[] args)
        {
            const string file = @"C:\Users\Mark\Documents\pagesuite\tickets\CS-1091 - Egmont quality\page flattened 2.pdf";

            using (Pdf pdf = new Pdf(file))
            {
                Page page = pdf.GetPage(1);

                CustomImageProcessor sp = new CustomImageProcessor();

                RecursiveStreamReader streamReader = new RecursiveStreamReader(sp);
                streamReader.ReadStreamRecursively(page);

                new Temp
                {
                    FileName = Path.GetFileName(file),
                    averageDPI = Math.Round(sp.images.Select(x => x.Dpi).Average(), 2),
                    maxDPI = Math.Round(sp.images.Select(x => x.Dpi).Max(), 2),
                    minDPI = Math.Round(sp.images.Select(x => x.Dpi).Min(), 2)
                }.Dump("Output");
            }
        }

        internal static void Main2(string[] args)
        {
            //const string file = @"C:\Users\Mark Tanner\Documents\clients\the sun\Jan 25th\page.pdf";
            List<Temp> output = new List<Temp>();

            foreach (string file in Directory.EnumerateFiles(@"C:\Users\Mark Tanner\Documents\clients\the sun\Jan 25th full"))
            {
                using (Pdf pdf = new Pdf(file))
                {
                    Page page = pdf.GetPage(1);

                    CustomImageProcessor sp = new CustomImageProcessor();

                    RecursiveStreamReader streamReader = new RecursiveStreamReader(sp);
                    streamReader.ReadStreamRecursively(page);

                    if (sp.images.Count == 0)
                    {
                        output.Add(new Temp
                        {
                            FileName = Path.GetFileName(file)
                        });
                    }
                    else
                    {
                        output.Add(new Temp
                        {
                            FileName = Path.GetFileName(file),
                            averageDPI = Math.Round(sp.images.Select(x => x.Dpi).Average(), 2),
                            maxDPI = Math.Round(sp.images.Select(x => x.Dpi).Max(), 2),
                            minDPI = Math.Round(sp.images.Select(x => x.Dpi).Min(), 2)
                        });
                    }
                }
            }

            Console.WriteLine(CSVFunctions.dataToCSV(output));
        }

        class ImageModel
        {
            public int ObjectNumber;
            public int NativeWidth;
            public int NativeHeight;
            public float Width;
            public float Height;
            public float X;
            public float Y;
            public float WidthPercent;
            public float HeightPercent;
            public float XPercent;
            public float YPercent;
            public float Dpi;
            public float compressionRatio;
        }

        class CustomImageProcessor : IStreamProcessor
        {
            private GraphicsStateProcessor gsp;
            private RecursiveStreamReader streamReader;
            public List<ImageModel> images;

            public void DidStartReadingStream(IStreamOwner streamOwner)
            {

            }

            public void ProcessOperation(Operation operation)
            {
                gsp.ProcessOperation(operation);

                if (operation.operatorName == "Do")
                {
                    string xObjectName = (Name)operation.operands[0];
                    object xObject = streamReader.Resources.GetObjectAtPath("XObject", xObjectName);

                    if (xObject is XObjectImage)
                    {
                        processImage(xObject as XObjectImage);
                    }
                }
            }

            private void processImage(XObjectImage image)
            {
                GraphicsState currentState = gsp.GetCurrentState();

                ImageModel model = new ImageModel
                {
                    ObjectNumber = image.Pdf.ReverseGet(image).objectNumber,
                    NativeWidth = Math.Abs(image.width),
                    NativeHeight = Math.Abs(image.height),
                    Width = Math.Abs(currentState.CurrentTransformationMatrix.Elements[0]),
                    Height = Math.Abs(currentState.CurrentTransformationMatrix.Elements[3]),
                    X = Math.Abs(currentState.CurrentTransformationMatrix.OffsetX),
                    Y = Math.Abs(currentState.CurrentTransformationMatrix.OffsetY),
                };

                model.Dpi = 72 * Math.Min(image.width / model.Width, image.height / model.Height);
                model.XPercent = 100 * model.X / streamReader.BoundingBox.Width;
                model.YPercent = 100 * model.Y / streamReader.BoundingBox.Height;
                model.WidthPercent = 100 * model.Width / streamReader.BoundingBox.Width;
                model.HeightPercent = 100 * model.Height / streamReader.BoundingBox.Height;
                
                int bitsPerComponent = image.UnderlyingDict.Get<int>("BitsPerComponent");
                ///probably need to do this based on color spaces instead
                int bytesPerPixel = (3 * bitsPerComponent) / 8;

                int rawByteSize = bytesPerPixel * image.width * image.height;
                int compressedSize = image.UnderlyingDict.Get<int>("Length");

                model.compressionRatio = (float)compressedSize / rawByteSize;

                images.Add(model);
            }

            public void WillFinishReadingPage()
            {

            }

            public void WillFinishReadingStream()
            {

            }

            public void WillStartReadingPage(RecursiveStreamReader parser)
            {
                images = new List<ImageModel>();
                streamReader = parser;
                gsp = new GraphicsStateProcessor(() => parser.Resources, parser.BoundingBox);
            }
        }
    }
}