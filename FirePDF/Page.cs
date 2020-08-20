using FirePDF.Model;
using FirePDF.Writing;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;

namespace FirePDF
{
    public class Page : HaveUnderlyingDict, IStreamOwner
    {
        public PdfResources Resources { get; }
        public RectangleF BoundingBox { get; }

        //TODO: do we need to check if the underlying dict is dirty too?
        public override bool IsDirty() => UnderlyingDict.IsDirty() || Resources.IsDirty();
        public bool IsOrphan { get; internal set; }

        Pdf IStreamOwner.Pdf => Pdf;

        public Page(Pdf owner, Size pageSize) : base(new PdfDictionary(owner, new Dictionary<Name, object>()))
        {
            IsOrphan = true;

            Resources = new PdfResources(new PdfDictionary(Pdf));
            UnderlyingDict.Set("Resources", Pdf.store.Add(Resources.UnderlyingDict));

            UnderlyingDict.Set("Type", (Name)"Page");
            UnderlyingDict.Set("MediaBox", new PdfList(Pdf, 0, 0, pageSize.Width, pageSize.Height));
        }

        internal Page(PdfDictionary pageDictionary) : this(pageDictionary, false) { }

        internal Page(PdfDictionary pageDictionary, bool isOrphan) : base(pageDictionary)
        {
            Resources = new PdfResources(UnderlyingDict.Get<PdfDictionary>("Resources"));
            IsOrphan = isOrphan;
            BoundingBox = UnderlyingDict.Get<PdfList>("MediaBox").AsRectangle();
        }

        public IEnumerable<ObjectReference> enumerateContentStreamReferences()
        {
            object contents = UnderlyingDict.Get("Contents", false);
            if (contents is ObjectReference)
            {
                yield return contents as ObjectReference;
            }
            else if (contents is PdfList list)
            {
                for (int i = 0; i < list.Count; i++)
                {
                    yield return list.Get<ObjectReference>(i, false);
                }
            }
            else
            {
                throw new Exception();
            }
        }

        public ObjectReference Serialize(PdfWriter writer)
        {
            throw new Exception("not handling saving yet");
            //if (resources.IsDirty)
            //{
            //    foreach (string dirtyObjectPath in resources.dirtyObjects)
            //    {
            //        string[] path = dirtyObjectPath.Split('/');
            //        object obj = resources.getObjectAtPath(path);

            //        ObjectReference objectRef = writer.writeIndirectObjectUsingNextFreeNumber(obj);
            //        resources.setObjectAtPath(objectRef, path);
            //    }

            //    resources.dirtyObjects.Clear();

            //    underlyingDict["Resources"] = resources.underlyingDict;
            //}

            //return writer.writeIndirectObjectUsingNextFreeNumber(underlyingDict);
        }

        public Stream GetStream()
        {
            MemoryStream compositeStream = new MemoryStream();

            object contents = UnderlyingDict.Get("Contents", true);

            if (contents is PdfStream)
            {
                using (Stream stream = (contents as PdfStream).GetDecompressedStream())
                {
                    stream.CopyTo(compositeStream);
                }
            }
            else if (contents is PdfList)
            {
                foreach (PdfStream pdfStream in (contents as PdfList).Cast<PdfStream>())
                {
                    using (Stream stream = pdfStream.GetDecompressedStream())
                    {
                        stream.CopyTo(compositeStream);
                    }
                }
            }
            else
            {
                throw new Exception();
            }

            compositeStream.Position = 0;
            return compositeStream;
        }
    }
}
