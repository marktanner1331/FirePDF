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
        public Dictionary<string, object> underlyingDict;
        public PDFResources resources { get; private set; }
        public RectangleF boundingBox { get; private set; }

        public Page(PDF pdf)
        {
            this.pdf = pdf;
            underlyingDict = getEmptyUnderlyingDict();
            throw new NotImplementedException();
        }
        
        public Page(PDF pdf, Dictionary<string, object> pageDictionary)
        {
            this.pdf = pdf;
            this.underlyingDict = pageDictionary;
            this.resources = new PDFResources(pdf, (Dictionary<string, object>)underlyingDict["Resources"]);
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
                using (Stream stream = PDFReader.readContentStream(pdf, objectReference))
                {
                    stream.CopyTo(compositeStream);
                }
            }

            compositeStream.Position = 0;
            return compositeStream;
        }

        private Dictionary<string, object> getEmptyUnderlyingDict()
        {
            //TODO finish getEmptyUnderlyingDict()
            //and do the same with other underlying dicts?
            return new Dictionary<string, object>
            {
                { "Contents", new List<ObjectReference>() }
            };
        }
    }
}
