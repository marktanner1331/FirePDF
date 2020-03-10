using FirePDF.Writing;
using System;
using System.Drawing;
using System.IO;

namespace FirePDF.Model
{
    public class XObjectForm : PdfStream, IStreamOwner
    {
        //private Stream stream;

        public PdfResources Resources { get; }
        public RectangleF BoundingBox { get; }

        //private bool isStreamDirty = false;
        //public bool IsDirty => isStreamDirty || resources.IsDirty;
        
        /// <summary>
        /// initializing a form xObject with the owning Pdf, its dictionary, and the offset to the start of the stream relative to the start of the Pdf
        /// </summary>
        public XObjectForm(Stream stream, PdfDictionary dict, long startOfStream) : base(stream, dict, startOfStream)
        {
            Resources = new PdfResources(this, UnderlyingDict.Get<PdfDictionary>("Resources"));
            BoundingBox = UnderlyingDict.Get<PdfList>("BBox").AsRectangle();
        }

        public ObjectReference Serialize(PdfWriter writer)
        {
            throw new Exception("not doing serialization yet");
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
            
            //if(stream == null)
            //{
            //    stream = Pdf.stream;
            //}

            //stream.Position = startOfStream;
            //return writer.writeIndirectObjectUsingNextFreeNumber(underlyingDict, stream);
        }

        public void WriteContentStream(Stream s)
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

        public Stream GetStream() => GetDecompressedStream();
    }
}
