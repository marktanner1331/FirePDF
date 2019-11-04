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
        private readonly PDF pdf;
        private readonly IStreamOwner owner;
        internal readonly PDFDictionary underlyingDict;

        private Dictionary<Name, object> cache;

        internal HashSet<string> dirtyObjects;
        internal bool isDirty => dirtyObjects.Count > 0;

        public PDFResources()
        {
            this.pdf = null;
            this.owner = null;
            this.underlyingDict = new PDFDictionary(null);
        }

        public PDFResources(PDF pdf, IStreamOwner owner, PDFDictionary underlyingDict)
        {
            this.pdf = pdf;
            this.owner = owner;
            this.underlyingDict = underlyingDict;
            
            this.cache = new Dictionary<Name, object>();
            this.dirtyObjects = new HashSet<string>();
        }
        
        public void overwriteXObject(XObjectForm form, string xObjectName)
        {
            //TODO get rid of this
            overwriteObject(form, "XObject", xObjectName);
        }

        public void overwriteObject(object obj, params string[] path)
        {
            string joinedPath = string.Join("/", path);
            cache[joinedPath] = obj;

            setObjectAsDirty(path);
        }
        
        /// <summary>
        /// returns all form xobjects found in the XObject dictionary
        /// does not return image xObjects
        /// </summary>
        public IEnumerable<string> listXObjectForms()
        {
            Dictionary<Name, object> xObjects = (Dictionary<Name, object>)getObjectAtPath("XObject");
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

        public bool isXObjectForm(string xObjectName)
        {
            object xObject = getObjectAtPath("XObject", xObjectName);
            return xObject is XObjectForm;
        }

        /// <summary>
        /// returns the object at the given path, or null if it cannot be found
        /// automatically resolves any indirect references
        /// </summary>
        public object getObjectAtPath(params string[] path)
        {
            string joinedPath = string.Join("/", path);
            if(cache.ContainsKey(joinedPath))
            {
                return cache[joinedPath];
            }

            object root = underlyingDict;
            foreach (string part in path)
            {
                if (root is Dictionary<Name, object>)
                {
                    Dictionary<Name, object> temp = (Dictionary<Name, object>)root;
                    if (temp.ContainsKey(part))
                    {
                        root = temp[part];
                    }
                    else
                    {
                        return null;
                    }
                }
                else if(root is PDFResources)
                {
                    root = ((PDFResources)root).underlyingDict[part];
                }
                else if(root is IStreamOwner)
                {
                    if(part != "Resources")
                    {
                        throw new Exception($"Accessing unknown property '{part}' on IStreamOWner");
                    }

                    root = ((IStreamOwner)root).resources;
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

            cache[joinedPath] = root;
            return root;
        }

        public void setObjectAsDirty(string[] path)
        {
            string joinedPath = string.Join("/", path);
            dirtyObjects.Add(joinedPath);

            object root = underlyingDict;
            for (int i = 0; i < path.Length - 1; i++)
            {
                string part = path[i];

                if (root is Dictionary<Name, object>)
                {
                    Dictionary<Name, object> temp = (Dictionary<Name, object>)root;
                    if (temp.ContainsKey(part))
                    {
                        root = temp[part];
                    }
                    else
                    {
                        throw new NotImplementedException();
                    }
                }
                else if (root is List<object>)
                {
                    int index;
                    if (int.TryParse(part, out index) == false)
                    {
                        throw new NotImplementedException();
                    }

                    List<object> temp = (List<object>)root;
                    if (temp.Count <= index)
                    {
                        throw new NotImplementedException();
                    }

                    root = temp[index];
                }

                if (root is ObjectReference)
                {
                    string[] subPath = path.Take(i + 1).ToArray();
                    root = getObjectAtPath(subPath);

                    string joinedSubPath = string.Join("/", subPath);
                    dirtyObjects.Add(joinedSubPath);
                }
            }
        }

        public void setObjectAtPath(ObjectReference objectReference, string[] path)
        {
            //TODO replace most of this with getObjectAtPAth(path.take(-1))
            object root = underlyingDict;
            for (int i = 0; i < path.Length - 1; i++)
            {
                string part = path[i];

                if (root is Dictionary<Name, object>)
                {
                    Dictionary<Name, object> temp = (Dictionary<Name, object>)root;
                    if (temp.ContainsKey(part))
                    {
                        root = temp[part];
                    }
                    else
                    {
                        throw new NotImplementedException();
                    }
                }
                else if (root is List<object>)
                {
                    int index;
                    if (int.TryParse(part, out index) == false)
                    {
                        throw new NotImplementedException();
                    }

                    List<object> temp = (List<object>)root;
                    if (temp.Count <= index)
                    {
                        throw new NotImplementedException();
                    }

                    root = temp[index];
                }

                if (root is ObjectReference)
                {
                    string[] subPath = path.Take(i + 1).ToArray();
                    root = getObjectAtPath(subPath);
                }
            }

            if (root is Dictionary<Name, object>)
            {
                Dictionary<Name, object> temp = (Dictionary<Name, object>)root;
                temp[path.Last()] = objectReference;
            }
            else if (root is List<object>)
            {
                int index;
                if (int.TryParse(path.Last(), out index) == false)
                {
                    throw new NotImplementedException();
                }

                List<object> temp = (List<object>)root;
                if (temp.Count <= index)
                {
                    throw new NotImplementedException();
                }

                temp[index] = objectReference;
            }
        }

        /// <summary>
        /// returns all form xobjects found in the XObject dictionary
        /// does not return form xObjects
        /// </summary>
        public IEnumerable<string> listXObjectImages()
        {
            Dictionary<Name, object> xObjects = (Dictionary<Name, object>)getObjectAtPath("XObject");
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
