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

namespace FirePDF.Model
{
    public class XObjectForm : IStreamOwner
    {
        public PDF pdf { get; private set; }
        public Dictionary<Name, object> underlyingDict;

        public long startOfStream;
        private Stream stream;

        public PDFResources resources { get; private set; }
        public RectangleF boundingBox { get; private set; }

        private bool isStreamDirty = false;
        public bool isDirty => isStreamDirty || resources.isDirty;

        public XObjectForm(PDF pdf)
        {
            this.pdf = pdf;

            //need to initialize a new underlyingDict and stream
            throw new NotImplementedException();
        }

        /// <summary>
        /// initializing a form xobject with the owning pdf, its dictionary, and the offset to the start of the stream relative to the start of the pdf
        /// </summary>
        public XObjectForm(PDF pdf, Dictionary<Name, object> dict, long startOfStream)
        {
            this.pdf = pdf;
            this.underlyingDict = dict;
            this.startOfStream = startOfStream;

            this.resources = new PDFResources(pdf, this, (Dictionary<Name, object>)underlyingDict["Resources"]);
            this.boundingBox = PDFReader.readRectangleFromArray((List<object>)underlyingDict["BBox"]);
        }

        public ObjectReference serialize(PDFWriter writer)
        {
            if (resources.isDirty)
            {
                foreach (string dirtyObjectPath in resources.dirtyObjects)
                {
                    string[] path = dirtyObjectPath.Split('/');
                    object obj = resources.getObjectAtPath(path);

                    ObjectReference objectRef = writer.writeIndirectObjectUsingNextFreeNumber(obj);
                    resources.setObjectAtPath(objectRef, path);
                }

                resources.dirtyObjects.Clear();

                underlyingDict["Resources"] = resources.underlyingDict;
            }
            
            if(stream == null)
            {
                stream = pdf.stream;
            }

            stream.Position = startOfStream;
            return writer.writeIndirectObjectUsingNextFreeNumber(underlyingDict, stream);
        }

        public void writeContentStream(Stream s)
        {
            stream = s;
            startOfStream = 0;
            
            underlyingDict.Remove("DecodeParms");
            underlyingDict.Remove("Filter");

            underlyingDict["Length"] = (int)s.Length;

            isStreamDirty = true;
        }

        public Stream readContentStream()
        {
            pdf.stream.Position = startOfStream;
            return PDFReader.decompressStream(pdf.stream, underlyingDict);
        }
    }
}
