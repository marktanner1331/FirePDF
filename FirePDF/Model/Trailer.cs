using FirePDF.Reading;
using FirePDF.Writing;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace FirePDF.Model
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
        
        public Trailer(PDFDictionary underylingDict)
        {
            foreach (var pair in underylingDict)
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

        public void serialize(PDFWriter writer)
        {
            writer.writeASCII("trailer");
            writer.writeNewLine();

            Dictionary<Name, object> underylingDict = new Dictionary<Name, object>();
            if (size != null)
            {
                underylingDict["Size"] = size.Value;
            }

            if (root != null)
            {
                underylingDict["Root"] = root;
            }

            if (info != null)
            {
                underylingDict["Info"] = info;
            }

            if (id != null)
            {
                throw new NotImplementedException();
            }

            if (prev != null)
            {
                underylingDict["Prev"] = prev.Value;
            }

            writer.writeDirectObject(underylingDict);
            writer.writeNewLine();
        }

    }
}