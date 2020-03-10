using FirePDF.Reading;
using System;
using System.Collections.Generic;
using System.IO;

namespace FirePDF.Model
{
    /// <summary>
    /// a class that wraps a Pdf object stream and provides methods to access objects in its compressed stream
    /// 7.5.7
    /// </summary>
    //TODO can this derive from PDFStream?
    public class PdfObjectStream : PdfStream
    {
        private readonly int n;
        private readonly int first;

        /// <summary>
        /// initializes the PDFObjectStream with a specific Pdf object
        /// </summary>
        public PdfObjectStream(Stream stream, PdfDictionary streamDict, long startOfStream) : base(stream, streamDict, startOfStream)
        {
            if(streamDict.Get<Name>("Type") != "ObjStm")
            {
                throw new Exception("Object is not an object stream");
            }

            n = streamDict.Get<int>("N");
            first = streamDict.Get<int>("First");
        }

        /// <summary>
        /// reads the N pairs of integers from the stream at the current position (should be 0)
        /// </summary>
        private Dictionary<int, int> ReadHeader(Stream stream)
        {
            Dictionary<int, int> pairs = new Dictionary<int, int>();
            for (int i = 0; i < n; i++)
            {
                int objectNumber = AsciiReader.ReadAsciiInteger(stream);
                stream.Position++;

                int offset = AsciiReader.ReadAsciiInteger(stream);
                stream.Position++;

                pairs[objectNumber] = offset;
            }

            return pairs;
        }

        public object ReadObject(int objectNumber)
        {
            stream.Position = startOfStream;
            using (Stream decompressedStream = PdfReader.DecompressStream(Pdf, stream, UnderlyingDict))
            {
                BinaryReader reader = new BinaryReader(decompressedStream);
                
                //key is the object number (object index)
                //the value is the offset, relative to the 'first' variable
                Dictionary<int, int> pairs = ReadHeader(decompressedStream);

                int offset = first + pairs[objectNumber];

                decompressedStream.Position = offset;

                return PdfReader.ReadObject(Pdf, decompressedStream);
            }
        }
    }
}
