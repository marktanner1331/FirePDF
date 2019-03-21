using FirePDF.Model;
using FirePDF.Reading;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FirePDF
{
    public class Page : IStreamOwner
    {
        public PDF pdf { get; private set; }
        public Dictionary<Name, object> underlyingDict;
        public PDFResources resources { get; private set; }
        public RectangleF boundingBox { get; private set; }

        public bool isDirty => resources.isDirty;

        public Page(PDF pdf)
        {
            this.pdf = pdf;
            underlyingDict = getEmptyUnderlyingDict();
            throw new NotImplementedException();
        }
        
        public Page(PDF pdf, Dictionary<Name, object> pageDictionary)
        {
            this.pdf = pdf;
            this.underlyingDict = pageDictionary;
            this.resources = new PDFResources(pdf, this, (Dictionary<Name, object>)underlyingDict["Resources"]);
        }

        public Stream readContentStream()
        {
            MemoryStream compositeStream = new MemoryStream();

            List<object> contents = (List<object>)underlyingDict["Contents"];
            if (contents == null)
            {
                throw new Exception();
            }

            foreach (ObjectReference objectReference in contents)
            {
                using (Stream stream = PDFReader.decompressStream(pdf, objectReference))
                {
                    stream.CopyTo(compositeStream);
                }
            }

            compositeStream.Position = 0;
            return compositeStream;
        }

        private Dictionary<Name, object> getEmptyUnderlyingDict()
        {
            //TODO finish getEmptyUnderlyingDict()
            //and do the same with other underlying dicts?
            return new Dictionary<Name, object>
            {
                { "Contents", new List<ObjectReference>() }
            };
        }
    }
}
