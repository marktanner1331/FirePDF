using FirePDF.Model;
using FirePDF.Reading;
using FirePDF.StreamHelpers;
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
    public class XObjectForm : PDFStream, IStreamOwner
    {
        public PDF pdf => base.pdf;
        //private Stream stream;

        public PDFResources resources { get; private set; }
        public RectangleF boundingBox { get; private set; }

        //private bool isStreamDirty = false;
        //public bool isDirty => isStreamDirty || resources.isDirty;
        
        /// <summary>
        /// initializing a form xobject with the owning pdf, its dictionary, and the offset to the start of the stream relative to the start of the pdf
        /// </summary>
        public XObjectForm(PDF pdf, PDFDictionary dict, long startOfStream) : base(pdf, dict, startOfStream)
        {
            this.resources = new PDFResources(pdf, this, underlyingDict.get<PDFDictionary>("Resources"));
            this.boundingBox = underlyingDict.get<PDFList>("BBox").asRectangle();
        }

        public ObjectReference serialize(PDFWriter writer)
        {
            throw new Exception("not doing serialization yet");
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
            
            //if(stream == null)
            //{
            //    stream = pdf.stream;
            //}

            //stream.Position = startOfStream;
            //return writer.writeIndirectObjectUsingNextFreeNumber(underlyingDict, stream);
        }

        public void writeContentStream(Stream s)
        {
            throw new Exception("not handling saving yet");

            //stream = new MemoryStream();

            //s.Position = 0;
            //ASCIIHexDecodeWriter.encode(s, stream);

            //startOfStream = 0;
            
            //underlyingDict["Filter"] = (Name)"ASCIIHexDecode";

            //underlyingDict["Length"] = (int)stream.Length;

            //isStreamDirty = true;
        }

        public Stream getStream() => getRawStream();
    }
}
