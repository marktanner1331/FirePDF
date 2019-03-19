using FirePDF.Model;
using FirePDF.Reading;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FirePDF.Model
{
    public class PDFResources
    {
        private PDF pdf;
        private Dictionary<string, object> underlyingDict;

        public PDFResources(PDF pdf, Dictionary<string, object> underlyingDict)
        {
            this.pdf = pdf;
            this.underlyingDict = underlyingDict;
        }
        
        /// <summary>
        /// returns all form xobjects found in the XObject dictionary
        /// does not return image xObjects
        /// </summary>
        public IEnumerable<XObjectForm> getXObjectForms()
        {
            Dictionary<string, object> xObjects = (Dictionary<string, object>)getObjectAtPath("XObject");
            if (xObjects == null)
            {
                yield break;
            }

            foreach (ObjectReference objectReference in xObjects.Values)
            {
                object xObject = PDFReader.readIndirectObject(pdf, objectReference);
                if (xObject is XObjectForm)
                {
                    yield return (XObjectForm)xObject;
                }
            }
        }

        public bool isXObjectImage(string xObjectName)
        {
            object xObject = getObjectAtPath("XObject", xObjectName);
            return xObject is XObjectImage;
        }

        /// <summary>
        /// returns the object at the given path, or null if it cannot be found
        /// automatically resolves any indirect references
        /// </summary>
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
                    root = PDFReader.readIndirectObject(pdf, (ObjectReference)root);
                }
            }

            return root;
        }

        /// <summary>
        /// returns all form xobjects found in the XObject dictionary
        /// does not return form xObjects
        /// </summary>
        public IEnumerable<XObjectImage> getXObjectImages()
        {
            Dictionary<string, object> xObjects = (Dictionary<string, object>)getObjectAtPath("XObject");
            if (xObjects == null)
            {
                yield break;
            }

            foreach (ObjectReference objectReference in xObjects.Values)
            {
                object xObject = PDFReader.readIndirectObject(pdf, objectReference);
                if (xObject is XObjectImage)
                {
                    yield return (XObjectImage)xObject;
                }
            }
        }

        /// <summary>
        /// returns the xObject image with the given name, or null if it cannot be found or is not an image
        /// </summary>
        public XObjectImage getXObjectImage(string xObjectName)
        {
            object xObject = getObjectAtPath("XObject", xObjectName);
            if (xObject is XObjectImage)
            {
                return (XObjectImage)xObject;
            }
            else
            {
                return null;
            }
        }
    }
}
