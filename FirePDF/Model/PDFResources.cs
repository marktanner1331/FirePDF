using FirePDF.Model;
using FirePDF.Reading;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FirePDF.Model
{
    public class PDFResources : IHaveUnderlyingDict
    {
        private readonly IStreamOwner owner;

        private Dictionary<Name, object> cache;

        internal HashSet<string> dirtyObjects;
        internal bool isDirty => dirtyObjects.Count > 0;
        
        public PDFResources(IStreamOwner owner, PDFDictionary underlyingDict) : base(underlyingDict)
        {
            this.owner = owner;
            
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
        /// returns the resource names of the fonts e.g. [/R16, /R17]
        /// </summary>
        /// <returns></returns>
        public List<Name> getFontResourceNames()
        {
            return underlyingDict.get<PDFDictionary>("Font").keys.ToList();
        }

        /// <summary>
        /// returns all form xobjects found in the XObject dictionary
        /// does not return image xObjects
        /// </summary>
        public IEnumerable<Name> listXObjectForms()
        {
            PDFDictionary xObjects = (PDFDictionary)getObjectAtPath("XObject");
            if (xObjects == null)
            {
                yield break;
            }

            foreach (Name key in xObjects.keys)
            {
                object xObject = xObjects.get<object>(key);
                if (xObject is XObjectForm)
                {
                    yield return key;
                }
            }
        }

        public bool isXObjectImage(string xObjectName)
        {
            object xObject = getObjectAtPath("XObject", xObjectName);
            return xObject is XObjectImage;
        }

        public Font getFont(Name name)
        {
            return (Font)getObjectAtPath("Font", name);
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
            //TODO make this a generic method like getObjectAtPath<Font>()?

            string joinedPath = string.Join("/", path);
            if(cache.ContainsKey(joinedPath))
            {
                return cache[joinedPath];
            }

            object root = underlyingDict;
            foreach (string part in path)
            {
                if (root is PDFDictionary)
                {
                    PDFDictionary temp = (PDFDictionary)root;
                    if (temp.containsKey(part))
                    {
                        root = temp.get<object>(part);
                    }
                    else
                    {
                        return null;
                    }
                }
                else if(root is PDFResources)
                {
                    root = ((PDFResources)root).underlyingDict.get<object>(part);
                }
                else if(root is IStreamOwner)
                {
                    if(part != "Resources")
                    {
                        throw new Exception($"Accessing unknown property '{part}' on IStreamOWner");
                    }

                    root = ((IStreamOwner)root).resources;
                }
                else if (root is PDFList)
                {
                    int index;
                    if (int.TryParse(part, out index) == false)
                    {
                        return null;
                    }

                    PDFList temp = (PDFList)root;
                    if (temp.count <= index)
                    {
                        return null;
                    }

                    root = temp.get<object>(index);
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

                if (root is PDFDictionary)
                {
                    PDFDictionary temp = (PDFDictionary)root;
                    if (temp.containsKey(part))
                    {
                        root = temp.get<object>(part);
                    }
                    else
                    {
                        throw new NotImplementedException();
                    }
                }
                else if (root is PDFList)
                {
                    int index;
                    if (int.TryParse(part, out index) == false)
                    {
                        throw new NotImplementedException();
                    }

                    PDFList temp = (PDFList)root;
                    if (temp.count <= index)
                    {
                        throw new NotImplementedException();
                    }

                    root = temp.get<object>(index);
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
            //TODO replace most of this with getObjectAtPath(path.take(-1))
            object root = underlyingDict;
            for (int i = 0; i < path.Length - 1; i++)
            {
                string part = path[i];

                if (root is PDFDictionary)
                {
                    PDFDictionary temp = (PDFDictionary)root;
                    if (temp.containsKey(part))
                    {
                        root = temp.get<object>(part);
                    }
                    else
                    {
                        throw new NotImplementedException();
                    }
                }
                else if (root is PDFList)
                {
                    int index;
                    if (int.TryParse(part, out index) == false)
                    {
                        throw new NotImplementedException();
                    }

                    PDFList temp = (PDFList)root;
                    if (temp.count <= index)
                    {
                        throw new NotImplementedException();
                    }

                    root = temp.get<object>(index);
                }
                
                if (root is ObjectReference)
                {
                    string[] subPath = path.Take(i + 1).ToArray();
                    root = getObjectAtPath(subPath);
                }
            }

            if (root is PDFDictionary)
            {
                PDFDictionary temp = (PDFDictionary)root;
                temp.set(path.Last(), objectReference);
            }
            else if (root is PDFList)
            {
                int index;
                if (int.TryParse(path.Last(), out index) == false)
                {
                    throw new NotImplementedException();
                }

                PDFList temp = (PDFList)root;
                if (temp.count <= index)
                {
                    throw new NotImplementedException();
                }
                
                temp.set(index, objectReference);
            }
        }

        /// <summary>
        /// returns all form xobjects found in the XObject dictionary
        /// does not return form xObjects
        /// </summary>
        public IEnumerable<Name> listXObjectImages()
        {
            PDFDictionary xObjects = (PDFDictionary)getObjectAtPath("XObject");
            if (xObjects == null)
            {
                yield break;
            }

            foreach (Name key in xObjects.keys)
            {
                object xObject = xObjects.get<object>(key);
                if (xObject is XObjectImage)
                {
                    yield return key;
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
