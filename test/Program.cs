using FirePDF;
using FirePDF.Model;
using FirePDF.Processors;
using FirePDF.Reading;
using FirePDF.Rendering;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing.Drawing2D;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using GraphicsState = FirePDF.Model.GraphicsState;

namespace test
{
    class Program
    {
        static void Main(string[] args)
        {
            extractTextTest();
            //fixCMAP();
        }

        private static void extractTextTest()
        {
            string file = @"C:\Users\Mark Tanner\scratch\orig fixed.pdf";
            using (PDF pdf = new PDF(file))
            {
                Page page = pdf.getPage(1);
                TextExtractor extractor = new TextExtractor();
                extractor.extractText(page);
            }
        }

        private static void fixCMAP()
        {
            string file = @"C:\Users\Mark Tanner\scratch\orig.pdf";
            string cmap = @"C:\Users\Mark Tanner\scratch\fixed cmap.txt";

            CMAP temp = CMAPReader.readCMAP(File.OpenRead(cmap));
            
            using (PDF pdf = new PDF(file))
            {
                ObjectReference newCMAP = pdf.addStream(new FileInfo(cmap));

                Page page = pdf.getPage(1);
                
                List<Name> fontNames = page.resources.getFontResourceNames();
                foreach (Name fontName in fontNames)
                {
                    Font font = page.resources.getFont(fontName);
                    if (font.baseFont == "HGEBGL+News706BT-RomanC")
                    {
                        font.setToUnicodeCMAP(newCMAP);
                    }
                }

                pdf.save(@"C:\Users\Mark Tanner\scratch\orig fixed.pdf", SaveType.Fresh);
            }
        }
    }
}
