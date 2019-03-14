using FirePDF.Reading;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FirePDF
{
    public class Page
    {
        public PDF pdf { get; private set; }
        private Dictionary<string, object> underlyingDict;

        public Page(PDF pdf)
        {
            this.pdf = pdf;

            underlyingDict = getEmptyUnderlyingDict();
        }

        private Dictionary<string, object> getEmptyUnderlyingDict()
        {
            //TODO finish this
            //and do the same with other underlying dicts?
            return new Dictionary<string, object>
            {
                { "Contents", new List<ObjectReference>() }
            };
        }

        public void fromDictionary(Dictionary<string, object> dict)
        {
            this.underlyingDict = dict;
        }

        public IEnumerable<XObjectForm> getXObjectForms()
        {
            Dictionary<string, object> xObjects = (Dictionary<string, object>)getObjectAtPath("Resources", "XObject");
            if (xObjects == null)
            {
                yield break;
            }

            foreach (ObjectReference objectReference in xObjects.Values)
            {
                object xObject = PDFReaderLayer1.readXObject(pdf, objectReference);
                if (xObject is XObjectForm)
                {
                    yield return (XObjectForm)xObject;
                }
            }
        }

        /// <summary>
        /// returns the object at the given path, or null if it cannot be found
        /// automatically resolves any indirect references before returning them
        /// </summary>
        public object getObjectAtPath(params string[] path)
        {
            if (path[0] == "Resources" && underlyingDict.ContainsKey("Resources") == false)
            {
                //resources are inherited from tree
                throw new NotImplementedException();
            }

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

        public IEnumerable<Image> getImages()
        {
            Dictionary<string, object> xObjects = (Dictionary<string, object>)getObjectAtPath("Resources", "XObject");
            if (xObjects == null)
            {
                yield break;
            }

            foreach (ObjectReference objectReference in xObjects.Values)
            {
                object xObject = PDFReaderLayer1.readXObject(pdf, objectReference);
                if (xObject is XObjectImage)
                {
                    XObjectImage image = (XObjectImage)xObject;
                    yield return image.getImage();
                }
            }
        }
    }
}
