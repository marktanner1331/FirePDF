using FirePDF;
using FirePDF.Model;
using FirePDF.Reading;
using System;
using System.Collections.Generic;
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
            string file = @"C:\Users\Mark Tanner\scratch\page 24 fixed.pdf";
            PDF pdf = new PDF(file);
            Page page = pdf.getPage(1);
            
            List<XObjectForm> forms = page.getXObjectForms().ToList();
            XObjectForm form = forms.First();

            Stream s = PDFReaderLayer2.readContentStream(form);
            List<Operation> operations = ContentStreamReader.readContentStream(s);

            GraphicsStateProcessor gsp = new GraphicsStateProcessor(form, form.getBoundingBox());
            foreach(Operation operation in operations)
            {
                gsp.processOperation(operation);
            }
        }
    }
}
