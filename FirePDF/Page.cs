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
        public readonly PDF pdf;
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


            //TODO probably want some kind of PDFList here
            //in case we have a list containing indirect references
            boundingBox = PDFReader.readRectangleFromArray(underlyingDict.get<List<object>>("MediaBox"));
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

        public Stream readContentStream()
        {
            MemoryStream compositeStream = new MemoryStream();

            object contents = underlyingDict.get<object>("Contents", false);
            
            //TODO this better
            //some kind of StreamObject
            //that PDFReader.readObject() returns, that we can get the stream from
            if(contents is ObjectReference)
            {
                using (Stream stream = PDFReader.decompressStream(pdf, contents as ObjectReference))
                {
                    stream.CopyTo(compositeStream);
                }
            }
            else
            {
                foreach (ObjectReference objectReference in contents as List<object>)
                {
                    using (Stream stream = PDFReader.decompressStream(pdf, objectReference))
                    {
                        stream.CopyTo(compositeStream);
                    }
                }
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
