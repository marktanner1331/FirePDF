using FirePDF.Model;
using FirePDF.Reading;
using FirePDF.Writing;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace FirePDF.Distilling
{
    class Distiller
    {
        public static void removeOperationsFromPage(Page page, Func<Operation, PdfResources, bool> callback)
        {
            Dictionary<ObjectReference, ObjectReference> refMap = new Dictionary<ObjectReference, ObjectReference>();

            foreach (ObjectReference objRef in page.ListContentStreams(true))
            {
                PdfStream stream = objRef.Get<PdfStream>();

                List<Operation> operations = ContentStreamReader.ReadOperationsFromStream(page.Pdf, stream.GetDecompressedStream());
                int count = operations.Count;

                operations = operations.Where(x => callback(x, page.Resources) == false).ToList();

                if (count != operations.Count)
                {
                    MemoryStream ms = new MemoryStream();
                    ContentStreamWriter.WriteOperationsToStream(ms, operations);
                    ms.Seek(0, SeekOrigin.Begin);
                    ObjectReference newStreamRef = page.Pdf.AddStream(ms);

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

        public static void removeAllImagesFromPage(Page page)
        {
            removeOperationsFromPage(page, (x, resources) =>
            {
                switch (x.operatorName)
                {
                    case "Do":
                        return resources.IsXObjectImage(x.GetOperandAsName(0));
                    case "BI":
                        return true;
                    default:
                        return false;
                }
            });

            foreach (IStreamOwner streamOwner in page.listIStreamOwners())
            {
                List<Name> imageNames = streamOwner.Resources.ListXObjectImageNames().ToList();
                foreach (Name imageName in imageNames)
                {
                    streamOwner.Resources.removeXObject(imageName);
                }
            }
        }
    }
}
