using FirePDF;
using FirePDF.Model;
using FirePDF.Rendering;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace test
{
    internal class Program
    {
        internal static void Main(string[] args)
        {
            //extractTextTest();
            //fixCMAP();

            const string file = @"C:\Users\Mark Tanner\scratch\press herald 2020-03-09\3.Pdf";
            //foreach(string file in Directory.EnumerateFiles(@"C:\Users\Mark Tanner\scratch\press herald 2020-03-09\"))
            //{
            using (Pdf pdf = new Pdf(file))
            {
                List<Font> fonts = pdf.GetAll<Font>();
                fonts = fonts.Where(x => x.baseFont.ToString().Contains("News706BT") && x.Encoding != null).ToList();
                foreach (Font font in fonts)
                {
                    if (font.ToUnicode != null)
                    {
                        Cmap cmap = font.ToUnicode;
                        if (cmap.CodeToUnicode(0xc0) == "i" || cmap.CodeToUnicode(0xc1) == "l")
                        {
                            PdfStream cmapData = font.UnderlyingDict.Get<PdfStream>("ToUnicode");
                            using (Stream stream = cmapData.GetDecompressedStream())
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

                                ObjectReference newCmapRef = pdf.AddStream(cmapString);
                                font.UnderlyingDict.Set("ToUnicode", newCmapRef);
                            }
                        }
                    }
                }

                pdf.Save(@"C:\Users\Mark Tanner\scratch\press herald 2020-03-09 fixed\3.Pdf", SaveType.Fresh);
            }
            //}
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

                List<Name> fontNames = page.Resources.GetFontResourceNames();
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
