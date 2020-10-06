using FirePDF;
using FirePDF.Model;
using FirePDF.Processors;
using FirePDF.Reading;
using FirePDF.Rendering;
using FirePDF.Text;
using FirePDF.Writing;
using Flattener;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace test
{
    internal class Program
    {
        internal static void Main(string[] args)
        {
            //const string file = @"C:\Users\Mark Tanner\Downloads\3108000_PageA_001.pdf";
            const string file = @"C:\Users\Mark Tanner\Downloads\orig.pdf";
            using (Pdf pdf = new Pdf(file))
            {
                Page page = pdf.GetPage(1);
               // Font font = page.Resources.GetFont("T1_5");
                Font font = page.Resources.GetFont("R33");
                string unicode = font.ReadUnicodeStringFromHexString(new PdfString(@"o\037ciated"));
            }
        }

        private static void ExtractTextTest()
        {
            const string file = @"C:\Users\Mark Tanner\scratch\orig fixed.Pdf";
            using (Pdf pdf = new Pdf(file))
            {
                Page page = pdf.GetPage(1);
                TextExtractor extractor = new TextExtractor();
                extractor.ExtractText(page);
            }
        }

        private static void FixCmap()
        {
            const string file = @"C:\Users\Mark Tanner\scratch\orig.Pdf";
            const string cmap = @"C:\Users\Mark Tanner\scratch\fixed cmap.txt";

            Cmap temp = new Cmap(File.OpenRead(cmap), true);

            using (Pdf pdf = new Pdf(file))
            {
                ObjectReference newCmap = pdf.AddStream(new FileInfo(cmap));

                Page page = pdf.GetPage(1);

                List<Name> fontNames = page.Resources.ListFontResourceNames();
                foreach (Name fontName in fontNames)
                {
                    Font font = page.Resources.GetFont(fontName);
                    if (font.baseFont == "HGEBGL+News706BT-RomanC")
                    {
                        font.SetToUnicodeCmap(newCmap);
                    }
                }

                pdf.Save(@"C:\Users\Mark Tanner\scratch\orig fixed.Pdf", SaveType.Fresh);
            }
        }
    }
}
