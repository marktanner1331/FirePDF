﻿using FirePDF.Reading;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FirePDF.Model
{
    /// <summary>
    /// a class that wraps a pdf object stream and provides methods to access objects in its compressed stream
    /// 7.5.7
    /// </summary>
    public class PDFObjectStream
    {
        private long startOfStream;
        private PDF pdf;
        private Stream stream;
        private PDFDictionary streamDict;

        private int n;
        private int first;

        /// <summary>
        /// initializes the PDFObjectStream with a specific pdf object
        /// </summary>
        public PDFObjectStream(PDF pdf, Stream stream, PDFDictionary streamDict, long startOfStream)
        {
            this.pdf = pdf;
            this.stream = stream;
            this.streamDict = streamDict;
            this.startOfStream = startOfStream;
            
            if(streamDict.get<Name>("Type") != "ObjStm")
            {
                throw new Exception("Object is not an object stream");
            }

            if(streamDict.ContainsKey("Extends"))
            {
                throw new NotImplementedException();
            }

            n = streamDict.get<int>("N");
            first = streamDict.get<int>("First");
        }

        /// <summary>
        /// reads the N pairs of integers from the stream at the current position (should be 0)
        /// </summary>
        private Dictionary<int, int> readHeader(Stream stream)
        {
            Dictionary<int, int> pairs = new Dictionary<int, int>();
            for (int i = 0; i < n; i++)
            {
                int objectNumber = ASCIIReader.readASCIIInteger(stream);
                stream.Position++;

                int offset = ASCIIReader.readASCIIInteger(stream);
                stream.Position++;

                pairs[objectNumber] = offset;
            }

            return pairs;
        }

        public object readObject(int objectNumber)
        {
            stream.Position = startOfStream;
            using (Stream decompressedStream = PDFReader.decompressStream(pdf, stream, streamDict))
            {
                BinaryReader reader = new BinaryReader(decompressedStream);
                
                //key is the object number (object index)
                //the value is the offset, relative to the 'first' variable
                Dictionary<int, int> pairs = readHeader(decompressedStream);

                int offset = first + pairs[objectNumber];

                decompressedStream.Position = offset;

                return PDFReader.readObject(pdf, decompressedStream);
            }
        }
    }
}
