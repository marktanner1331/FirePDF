using FirePDF;
using FirePDF.Distilling;
using FirePDF.Model;
using FirePDF.Processors;
using FirePDF.Reading;
using FirePDF.Rendering;
using FirePDF.StreamPartFunctions;
using FirePDF.Util;
using FirePDF.Writing;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace test
{
    class Program
    {
        static void Main(string[] args)
        {
            string file = @"C:\Users\Mark Tanner\scratch\kuier 1.pdf";
            PDF pdf = new PDF(file);
            Page page = pdf.getPage(1);

            File.Delete(@"C:\Users\Mark Tanner\scratch\kuier 1 write test.pdf");
            using (FileStream fs = new FileStream(@"C:\Users\Mark Tanner\scratch\kuier 1 write test.pdf", FileMode.CreateNew))
            {
                pdf.stream.Seek(0, SeekOrigin.Begin);
                pdf.stream.CopyTo(fs);

                using (PDFWriter writer = new PDFWriter(fs, true))
                {
                    string formName = page.resources.listXObjectForms().First();
                    XObjectForm form = page.resources.getXObjectForm(formName);

                    Stream s = form.readContentStream();
                    List<Operation> operations = ContentStreamReader.readOperationsFromStream(s);

                    ModificationEngine me = new ModificationEngine();
                    me.increaseImageDimensionsByOnePixel = true;
                    me.increaseClippingPathsByOnePixel = true;
                    //me.removeImageClippingPaths = true;
                    operations = me.run(form, operations);

                    s = new MemoryStream();
                    ContentStreamWriter.writeOperationsToStream(s, operations);
                    form.writeContentStream(s);

                    page.resources.overwriteXObject(form, formName);
                    writer.updatePDF(pdf);
                }
            }
        }
    }
}
