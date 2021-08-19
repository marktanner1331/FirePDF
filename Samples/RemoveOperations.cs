using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using FirePDF;
using FirePDF.Model;
using FirePDF.Reading;
using FirePDF.Writing;

namespace Samples
{
    class RemoveOperations
    {
        internal static void Main(string[] args)
        {
            const string file = @"C:\Users\Mark\Downloads\News_6_GV5KMABF6.1+GV5KMABF6.1.pdf";
            using (Pdf pdf = new Pdf(file))
            {
                Page page = pdf.GetPage(1);
                var k = page.listIStreamOwners();

                foreach (var g in k)
                {
                    using (Stream s = g.GetStream())
                    {
                        List<Operation> operations = ContentStreamReader.ReadOperationsFromStream(pdf, s);
                        int originalSize = operations.Count;

                        operations = operations.Where(x =>
                        {
                            if (x.operatorName == "scn" && x.operands[0] is Name && (Name)x.operands[0] == "R15")
                            {
                                return false;
                            }
                            
                            if (x.operatorName == "cs" && x.operands[0] is Name && (Name)x.operands[0] == "R16")
                            {
                                return false;
                            }

                            return true;
                        }).ToList();

                        if (originalSize == operations.Count)
                        {
                            continue;
                        }

                        //gotta keep this stream open until we hit save
                        MemoryStream ms = new MemoryStream();

                        ContentStreamWriter.WriteOperationsToStream(ms, operations);
                        ms.Seek(0, SeekOrigin.Begin);
                        
                        if (g is XObjectForm form)
                        {
                            form.UpdateStream(ms);
                        }
                    }
                }

                pdf.Save(@"C:\Users\Mark\Downloads\News_6_GV5KMABF6.1+GV5KMABF6.1 fixed.pdf", SaveType.Fresh);
            }
        }
    }
}
