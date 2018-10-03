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
            string keyword = FileReader.readASCIIString(stream, 7);
            if(keyword != "trailer")
            {
                throw new Exception("trailer not found at current position");
            }

            PDFReaderLevel1.skipOverWhiteSpace(stream);
            Dictionary<string, object> dict = PDFReaderLevel1.readDictionary(stream);

            foreach(var pair in dict)
            {
                switch(pair.Key)
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
                        id = ((List<object>)pair.Value).Cast<byte[]>().ToList();
                        break;
                    case "Prev":
                        prev = (int)pair.Value;
                        break;
                }
            }
        }
    }
}