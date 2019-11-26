using FirePDF.Model;
using FirePDF.Reading;
using FirePDF.Writing;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FirePDF.Util;

namespace FirePDF
{
    public class Page : IHaveUnderlyingDict, IStreamOwner
    {
        public PDFResources resources { get; private set; }
        public RectangleF boundingBox { get; private set; }

        public bool isDirty => resources.isDirty;
        public bool isOrphan { get; internal set; }

        PDF IStreamOwner.pdf => pdf;

        public Page(PDF owner, Size pageSize) : base(new PDFDictionary(owner, new Dictionary<Name, object>()))
        {
            this.isOrphan = true;

            this.resources = new PDFResources(this, new PDFDictionary(pdf));
            this.underlyingDict.set("Resources", pdf.store.add(resources.underlyingDict, false));
            
            this.underlyingDict.set("Type", (Name)"Page");
            this.underlyingDict.set("MediaBox", new PDFList(pdf, 0, 0, pageSize.Width, pageSize.Height));
        }

        internal Page(PDFDictionary pageDictionary) : this(pageDictionary, false) { }

        internal Page(PDFDictionary pageDictionary, bool isOrphan) : base(pageDictionary)
        {
            this.resources = new PDFResources(this, underlyingDict.get<PDFDictionary>("Resources"));
            this.isOrphan = isOrphan;
            boundingBox = underlyingDict.get<PDFList>("MediaBox").asRectangle();
        }

        public ObjectReference serialize(PDFWriter writer)
        {
            throw new Exception("not handling saving yet");
            return null;
            //if (resources.isDirty)
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

        public Stream getStream()
        {
            MemoryStream compositeStream = new MemoryStream();

            object contents = underlyingDict.get<object>("Contents", true);

            if (contents is PDFStream)
            {
                using (Stream stream = (contents as PDFStream).getDecompressedStream())
                {
                    stream.CopyTo(compositeStream);
                }
            }
            else if (contents is PDFList)
            {
                foreach (PDFStream pdfStream in (contents as PDFList).cast<PDFStream>())
                {
                    using (Stream stream = pdfStream.getDecompressedStream())
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
