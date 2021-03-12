using FirePDF;
using FirePDF.Model;
using FirePDF.Processors;
using FirePDF.Reading;
using FirePDF.Rendering;
using FirePDF.Text;
using FirePDF.Writing;
using Flattener;
using ObjectDumper;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.IO;
using System.Linq;

namespace test
{
    internal class Program
    {
        internal static void Main(string[] args)
        {
            const string file = @"C:\Users\Mark Tanner\Documents\clients\media24\CW-3368\die burger 3 page.pdf";
            
            using (Pdf pdf = new Pdf(file))
            {
                var k = pdf.Get<Object>(174, 0);
                
                //Page page = pdf.GetPage(1);

                //PdfToHhml renderer = new PdfToHhml();

                //StreamProcessor sp = new StreamProcessor(renderer);
                //RecursiveStreamReader streamReader = new RecursiveStreamReader(sp);
                //streamReader.ReadStreamRecursively(page);
            }
        }

        class PdfToHhml : Renderer
        {


            public override void WillStartRenderingPage(RectangleF boundingBox, Func<FirePDF.Model.GraphicsState> getGraphicsState)
            {
                base.WillStartRenderingPage(boundingBox, getGraphicsState);
            }

            public override void DrawImage(XObjectImage image)
            {
                throw new NotImplementedException();
            }

            public override void DrawText(byte[] text)
            {
                throw new NotImplementedException();
            }

            public override void FillAndStrokePath(GraphicsPath path)
            {
                throw new NotImplementedException();
            }

            public override void FillPath(GraphicsPath path)
            {
                throw new NotImplementedException();
            }

            public override void StrokePath(GraphicsPath path)
            {
                throw new NotImplementedException();
            }
        }
    }
}
