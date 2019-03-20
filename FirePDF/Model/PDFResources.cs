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

        private Dictionary<string, object> updates;

        public PDFResources(PDF pdf, Dictionary<string, object> underlyingDict)
        {
            this.pdf = pdf;
            this.underlyingDict = underlyingDict;
            this.updates = new Dictionary<string, object>();
        }

        public void overwriteXObject(XObjectForm form, string xObjectName)
        {
            overwriteObject(form, "XObject", xObjectName);
        }

        public void overwriteObject(object obj, params string[] path)
        {
            updates[string.Join("/", path)] = obj;
        }
        
        /// <summary>
        /// returns all form xobjects found in the XObject dictionary
        /// does not return image xObjects
        /// </summary>
        public IEnumerable<string> listXObjectForms()
        {
            Dictionary<string, object> xObjects = (Dictionary<string, object>)getObjectAtPath("XObject");
            if (xObjects == null)
            {
                yield break;
            }

            foreach (var pair in xObjects)
            {
                object xObject = PDFReader.readIndirectObject(pdf, (ObjectReference)pair.Value);
                if (xObject is XObjectForm)
                {
                    yield return pair.Key;
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
        public IEnumerable<string> listXObjectImages()
        {
            Dictionary<string, object> xObjects = (Dictionary<string, object>)getObjectAtPath("XObject");
            if (xObjects == null)
            {
                yield break;
            }

            foreach (var pair in xObjects)
            {
                object xObject = PDFReader.readIndirectObject(pdf, (ObjectReference)pair.Value);
                if (xObject is XObjectImage)
                {
                    yield return pair.Key;
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

        /// <summary>
        /// returns the xObject form with the given name, or null if it cannot be found or is not a form
        /// </summary>
        public XObjectForm getXObjectForm(string xObjectName)
        {
            object xObject = getObjectAtPath("XObject", xObjectName);
            if (xObject is XObjectForm)
            {
                return (XObjectForm)xObject;
            }
            else
            {
                return null;
            }
        }
    }
}
