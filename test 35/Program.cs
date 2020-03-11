using FirePDF;
using FirePDF.Model;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace test_35
{
    class Program
    {
        static void Main(string[] args)
        {
            using (Pdf pdf = new Pdf(new FileInfo(@"C:\Users\Mark Tanner\scratch\page.pdf")))
            {
                var k = pdf.Get<object>(60, 0);
            }
        }

        public static void fixPDFFile(FileInfo pdfFile)
        {
            using (Pdf pdf = new Pdf(pdfFile))
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
                                if (oldLength == cmapString.Length)
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

                pdf.Save(pdfFile.FullName.Replace(".pdf", "_fixed.pdf"), SaveType.Fresh);
            }

            //File.Delete(pdfFile.FullName);
            //File.Move(pdfFile.FullName.Replace(".pdf", "_fixed.pdf"), pdfFile.FullName);
        }
    }
}
