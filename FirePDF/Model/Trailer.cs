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
            foreach (Name key in underylingDict.keys)
            {
                switch (key)
                {
                    case "Size":
                        size = underylingDict.get<int>(key);
                        break;
                    case "Root":
                        root = underylingDict.get<ObjectReference>(key);
                        break;
                    case "Info":
                        info = underylingDict.get<ObjectReference>(key);
                        break;
                    case "ID":
                        List<object> tempList = underylingDict.get<List<object>>(key);
                        if (tempList.All(X => X is string))
                        {
                            id = tempList
                                .Select(x => ((string)x)
                                    .ToCharArray()
                                    .Select(y => (byte)y)
                                    .ToArray())
                                .ToList();
                        }
                        else
                        {
                            id = tempList.Cast<byte[]>().ToList();
                        }
                        break;
                    case "Prev":
                        prev = underylingDict.get<int>(key);
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