using FirePDF;
using FirePDF.Model;
using FirePDF.Processors;
using FirePDF.Reading;
using FirePDF.Rendering;
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
            run();
        }

        private static void run()
        {
            string file = @"C:\Users\Mark Tanner\Documents\clients\seattle times\01.pdf";
            using (Pdf pdf = new Pdf(file))
            {
                removeOperation(pdf, (x, resources) =>
                {
                    if (x.operatorName != "Do")
                    {
                        return false;
                    }

                    return resources.IsXObjectImage(x.GetOperandAsName(0));
                });

                pdf.Save(file.Replace(@"\seattle times\", @"\seattle times\no images\"), SaveType.Fresh);
            }
        }

        private static void removeOperation(Pdf pdf, Func<Operation, PdfResources, bool> callback)
        {
            foreach (Page page in pdf)
            {
                Dictionary<ObjectReference, ObjectReference> refMap = new Dictionary<ObjectReference, ObjectReference>();
                Queue<ObjectReference> streams = new Queue<ObjectReference>(page.enumerateContentStreamReferences());
                HashSet<ObjectReference> modifiedStreams = new HashSet<ObjectReference>();

                while (streams.Count > 0)
                {
                    ObjectReference objRef = streams.Dequeue();
                    if (modifiedStreams.Contains(objRef))
                    {
                        continue;
                    }

                    modifiedStreams.Add(objRef);
                    PdfStream stream = objRef.Get<PdfStream>();

                    List<Operation> operations = ContentStreamReader.ReadOperationsFromStream(pdf, stream.GetDecompressedStream());
                    int count = operations.Count;

                    operations = operations.Where(x => callback(x, page.Resources) == false).ToList();

                    if (count != operations.Count)
                    {
                        MemoryStream ms = new MemoryStream();
                        ContentStreamWriter.WriteOperationsToStream(ms, operations);
                        ms.Seek(0, SeekOrigin.Begin);
                        ObjectReference newStreamRef = pdf.AddStream(ms);

                        refMap[objRef] = newStreamRef;
                    }
                }

                page.SwapReferences(x =>
                {
                    if (refMap.ContainsKey(x))
                    {
                        return refMap[x];
                    }
                    else
                    {
                        return x;
                    }
                });
            }
        }

        private static void runAll()
        {
            const string folder = @"C:\Users\Mark Tanner\Documents\clients\seattle times\";
            foreach (string file in Directory.EnumerateFiles(folder).Skip(26))
            {
                using (Pdf pdf = new Pdf(file))
                {
                    Page page = pdf.GetPage(1);
                    PdfList contents = page.UnderlyingDict.Get<PdfList>("Contents");
                    Dictionary<ObjectReference, ObjectReference> refMap = new Dictionary<ObjectReference, ObjectReference>();

                    for (int i = 0; i < contents.Count; i++)
                    {
                        ObjectReference objRef = contents.Get<ObjectReference>(i, false);
                        PdfStream stream = objRef.Get<PdfStream>();

                        List<Operation> operations = ContentStreamReader.ReadOperationsFromStream(pdf, stream.GetDecompressedStream());
                        if (operations.Any(x => x.operatorName == "sh"))
                        {
                            operations = operations.Where(x => x.operatorName != "sh").ToList();
                            MemoryStream ms = new MemoryStream();
                            ContentStreamWriter.WriteOperationsToStream(ms, operations);
                            ms.Seek(0, SeekOrigin.Begin);
                            ObjectReference newStreamRef = pdf.AddStream(ms);

                            refMap[objRef] = newStreamRef;
                        }
                    }

                    page.SwapReferences(x =>
                    {
                        if (refMap.ContainsKey(x))
                        {
                            return refMap[x];
                        }
                        else
                        {
                            return x;
                        }
                    });

                    pdf.Save(file.Replace(@"\seattle times\", @"\seattle times\fixed\"), SaveType.Update);
                }
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
