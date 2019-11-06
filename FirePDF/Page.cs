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
    public class Page : IStreamOwner
    {
        public PDF pdf { get; }
        public PDFDictionary underlyingDict;
        public PDFResources resources { get; private set; }
        public RectangleF boundingBox { get; private set; }

        public bool isDirty => resources.isDirty;
        
        public Page(PDF pdf)
        {
            this.pdf = pdf;
            underlyingDict = getEmptyUnderlyingDict();
            throw new NotImplementedException();
        }

        public Page(PDF pdf, PDFDictionary pageDictionary)
        {
            this.pdf = pdf;
            this.underlyingDict = pageDictionary;

            this.resources = new PDFResources(pdf, this, underlyingDict.get<PDFDictionary>("Resources"));

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
            
            if(contents is PDFStream)
            {
                using (Stream stream = (contents as PDFStream).getRawStream())
                {
                    stream.CopyTo(compositeStream);
                }
            }
            else if(contents is PDFList)
            {
                foreach (PDFStream pdfStream in (contents as PDFList).cast<PDFStream>())
                {
                    using (Stream stream = pdfStream.getRawStream())
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

        private PDFDictionary getEmptyUnderlyingDict()
        {
            throw new Exception("not done yet");

            //TODO finish getEmptyUnderlyingDict()
            //and do the same with other underlying dicts?
            //return new Dictionary<Name, object>
            //{
            //    { "Contents", new List<ObjectReference>() }
            //};
        }
    }
}
