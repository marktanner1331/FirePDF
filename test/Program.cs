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
            //extractTextTest();
            //fixCMAP();

            string file = @"C:\Users\Mark Tanner\scratch\press herald 2020-03-09\3.pdf";
            //foreach(string file in Directory.EnumerateFiles(@"C:\Users\Mark Tanner\scratch\press herald 2020-03-09\"))
            //{
            using (PDF pdf = new PDF(file))
            {
                List<Font> fonts = pdf.getAll<Font>();
                fonts = fonts.Where(x => x.baseFont.ToString().Contains("News706BT") && x.encoding != null).ToList();
                foreach (Font font in fonts)
                {
                    if (font.toUnicode != null)
                    {
                        CMAP cmap = font.toUnicode;
                        if (cmap.codeToUnicode(0xc0) == "i" || cmap.codeToUnicode(0xc1) == "l")
                        {
                            PDFStream cmapData = font.underlyingDict.get<PDFStream>("ToUnicode");
                            using (Stream stream = cmapData.getDecompressedStream())
                            using (StreamReader streamReader = new StreamReader(stream))
                            {
                                string cmapString = streamReader.ReadToEnd();

                                int oldLength = cmapString.Length;
                                cmapString = cmapString.Replace("<00c0><00c0><0069>", "<00c0><00c0><00660069>");
                                cmapString = cmapString.Replace("<00c1><00c1><006c>", "<00c1><00c1><0066006c>");
                                if(oldLength == cmapString.Length)
                                {
                                    //no change to cmap, so no point saving it
                                    continue;
                                }

                                ObjectReference newCmapRef = pdf.addStream(cmapString);
                                font.underlyingDict.set("ToUnicode", newCmapRef);
                            }
                        }
                    }
                }

                pdf.save(@"C:\Users\Mark Tanner\scratch\press herald 2020-03-09 fixed\3.pdf", SaveType.Fresh);
            }
            //}
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

            CMAP temp = new CMAP(File.OpenRead(cmap), true);

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
