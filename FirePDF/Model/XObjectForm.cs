using FirePDF.Model;
using FirePDF.Reading;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FirePDF.Model
{
    public class XObjectForm : IStreamOwner
    {
        public PDF pdf { get; private set; }
        internal Dictionary<string, object> underlyingDict;
        internal long startOfStream;

        public XObjectForm(PDF pdf)
        {
            this.pdf = pdf;

            //need to initialize a new underlyingDict and stream
            throw new NotImplementedException();
        }

        /// <summary>
        /// initializing a form xobject with the owning pdf, its dictionary, and the offset to the start of the stream relative to the start of the pdf
        /// </summary>
        public XObjectForm(PDF pdf, Dictionary<string, object> dict, long startOfStream)
        {
            this.pdf = pdf;
            this.underlyingDict = dict;
            this.startOfStream = startOfStream;
        }

        public Rectangle getBoundingBox()
        {
            return new Rectangle((List<object>)underlyingDict["BBox"]);
        }

        /// <summary>
        /// returns the object at the given path, or null if it cannot be found
        /// automatically resolves any indirect references before returning them
        /// </summary>
        /// TODO deprecate this as we can use PDFResources instead for most things
        public object getObjectAtPath(params string[] path)
        {
            object root = underlyingDict;
            foreach (string part in path)
            {
                if (root is Dictionary<string, object>)
                {
                    Dictionary<string, object> temp = (Dictionary<string, object>)root;
                    if (temp.ContainsKey(part))
                    {
                        root = temp[part];
                    }
                    else
                    {
                        return null;
                    }
                }
                else if (root is List<object>)
                {
                    int index;
                    if (int.TryParse(part, out index) == false)
                    {
                        return null;
                    }

                    List<object> temp = (List<object>)root;
                    if (temp.Count <= index)
                    {
                        return null;
                    }

                    root = temp[index];
                }

                if (root is ObjectReference)
                {
                    root = PDFReaderLayer1.readIndirectObject(pdf, (ObjectReference)root);
                }
            }

            return root;
        }

        public PDFResources getResources()
        {
            Dictionary<string, object> resources = (Dictionary<string, object>)underlyingDict["Resources"];
            return new PDFResources(pdf, resources);
        }
    }
}
