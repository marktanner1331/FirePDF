using FirePDF.Reading;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace FirePDF
{
    public class Trailer
    {
        public int? size;
        public ObjectReference root;
        public ObjectReference info;
        public List<byte[]> id;
        public int? prev;

        public Trailer()
        {

        }

        public void fromStream(Stream stream)
        {
            string keyword = ASCIIReader.readASCIIString(stream, 7);
            if (keyword != "trailer")
            {
                throw new Exception("trailer not found at current position");
            }

            PDFReaderLayer1.skipOverWhiteSpace(stream);
            Dictionary<string, object> dict = PDFReaderLayer1.readDictionary(stream);

            fromDictionary(dict);
        }

        public void fromDictionary(Dictionary<string, object> dict)
        {
            foreach (var pair in dict)
            {
                switch (pair.Key)
                {
                    case "Size":
                        size = (int)pair.Value;
                        break;
                    case "Root":
                        root = (ObjectReference)pair.Value;
                        break;
                    case "Info":
                        info = (ObjectReference)pair.Value;
                        break;
                    case "ID":
                        if (((List<object>)pair.Value).All(X => X is string))
                        {
                            id = ((List<object>)pair.Value)
                                .Select(x => ((string)x)
                                    .ToCharArray()
                                    .Select(y => (byte)y)
                                    .ToArray())
                                .ToList();
                        }
                        else
                        {
                            id = ((List<object>)pair.Value).Cast<byte[]>().ToList();
                        }
                        break;
                    case "Prev":
                        prev = (int)pair.Value;
                        break;
                }
            }
        }
    }
}