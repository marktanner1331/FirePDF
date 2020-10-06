using FirePDF.Text;
using System;
using System.Collections.Generic;
using System.Linq;

namespace FirePDF.Model
{
    public class PdfResources : HaveUnderlyingDict
    {
        private readonly Dictionary<Name, object> cache;

        internal HashSet<string> dirtyObjects;
        public override bool IsDirty() => dirtyObjects.Count > 0;

        public PdfResources(PdfDictionary underlyingDict) : base(underlyingDict)
        {
            cache = new Dictionary<Name, object>();
            dirtyObjects = new HashSet<string>();
        }

        public void OverwriteXObject(XObjectForm form, string xObjectName)
        {
            //TODO get rid of this
            OverwriteObject(form, "XObject", xObjectName);
        }

        public void OverwriteObject(object obj, params string[] path)
        {
            string joinedPath = string.Join("/", path);
            cache[joinedPath] = obj;

            SetObjectAsDirty(path);
        }

        /// <summary>
        /// returns the resource names of the fonts e.g. [/R16, /R17]
        /// </summary>
        /// <returns></returns>
        public List<Name> ListFontResourceNames()
        {
            return UnderlyingDict.Get<PdfDictionary>("Font").Keys.ToList();
        }

        public void removeFont(Name name)
        {
            PdfDictionary fonts = (PdfDictionary)GetObjectAtPath("Font");
            if (fonts == null)
            {
                return;
            }

            fonts.RemoveEntry(name);
        }

        public void removeXObject(Name name)
        {
            PdfDictionary xObjects = (PdfDictionary)GetObjectAtPath("XObject");
            if (xObjects == null)
            {
                return;
            }

            xObjects.RemoveEntry(name);
        }

        /// <summary>
        /// returns all form xobjects found in the XObject dictionary
        /// does not return image xObjects
        /// </summary>
        public IEnumerable<ObjectReference> ListFormXObjects()
        {
            PdfDictionary xObjects = (PdfDictionary)GetObjectAtPath("XObject");
            if (xObjects == null)
            {
                yield break;
            }

            foreach (KeyValuePair<Name, object> entry in xObjects)
            {
                ObjectReference objRef = entry.Value as ObjectReference;
                if (objRef.Get<object>() is XObjectForm)
                {
                    yield return objRef;
                }
            }
        }

        public IEnumerable<ObjectReference> ListImages()
        {
            PdfDictionary xObjects = (PdfDictionary)GetObjectAtPath("XObject");
            if (xObjects == null)
            {
                yield break;
            }

            foreach (KeyValuePair<Name, object> entry in xObjects)
            {
                ObjectReference objRef = entry.Value as ObjectReference;
                if (objRef.Get<object>() is XObjectForm)
                {
                    yield return objRef;
                }
            }
        }

        /// <summary>
        /// returns all form xobjects found in the XObject dictionary
        /// does not return image xObjects
        /// </summary>
        public IEnumerable<Name> ListFormXObjectNames()
        {
            PdfDictionary xObjects = (PdfDictionary)GetObjectAtPath("XObject");
            if (xObjects == null)
            {
                yield break;
            }

            foreach (Name key in xObjects.Keys)
            {
                object xObject = xObjects.Get<object>(key);
                if (xObject is XObjectForm)
                {
                    yield return key;
                }
            }
        }

        public bool IsXObjectImage(string xObjectName)
        {
            object xObject = GetObjectAtPath("XObject", xObjectName);
            return xObject is XObjectImage;
        }

        public Font GetFont(Name name)
        {
            return (Font)GetObjectAtPath("Font", name);
        }

        public bool IsXObjectForm(string xObjectName)
        {
            object xObject = GetObjectAtPath("XObject", xObjectName);
            return xObject is XObjectForm;
        }

        /// <summary>
        /// returns the object at the given path, or null if it cannot be found
        /// automatically resolves any indirect references
        /// </summary>
        public object GetObjectAtPath(params string[] path)
        {
            //TODO make this a generic method like getObjectAtPath<Font>()?

            string joinedPath = string.Join("/", path);
            if (cache.ContainsKey(joinedPath))
            {
                return cache[joinedPath];
            }

            object root = UnderlyingDict;
            foreach (string part in path)
            {
                switch (root)
                {
                    case PdfDictionary dictionary when dictionary.ContainsKey(part):
                        root = dictionary.Get<object>(part);
                        break;
                    case PdfDictionary dictionary:
                        return null;
                    case PdfResources resources:
                        root = resources.UnderlyingDict.Get<object>(part);
                        break;
                    case IStreamOwner streamOwner when part != "Resources":
                        throw new Exception($"Accessing unknown property '{part}' on IStreamOwner");
                    case IStreamOwner streamOwner:
                        root = streamOwner.Resources;
                        break;
                    case PdfList temp:
                        {
                            if (int.TryParse(part, out int index) == false)
                            {
                                return null;
                            }

                            if (temp.Count <= index)
                            {
                                return null;
                            }

                            root = temp.Get<object>(index);
                            break;
                        }
                }
            }

            cache[joinedPath] = root;
            return root;
        }

        public void SetObjectAsDirty(string[] path)
        {
            string joinedPath = string.Join("/", path);
            dirtyObjects.Add(joinedPath);

            object root = UnderlyingDict;
            for (int i = 0; i < path.Length - 1; i++)
            {
                string part = path[i];

                if (root is PdfDictionary)
                {
                    PdfDictionary temp = (PdfDictionary)root;
                    if (temp.ContainsKey(part))
                    {
                        root = temp.Get<object>(part);
                    }
                    else
                    {
                        throw new NotImplementedException();
                    }
                }
                else if (root is PdfList)
                {
                    int index;
                    if (int.TryParse(part, out index) == false)
                    {
                        throw new NotImplementedException();
                    }

                    PdfList temp = (PdfList)root;
                    if (temp.Count <= index)
                    {
                        throw new NotImplementedException();
                    }

                    root = temp.Get<object>(index);
                }

                if (root is ObjectReference)
                {
                    string[] subPath = path.Take(i + 1).ToArray();
                    root = GetObjectAtPath(subPath);

                    string joinedSubPath = string.Join("/", subPath);
                    dirtyObjects.Add(joinedSubPath);
                }
            }
        }

        public void SetObjectAtPath(ObjectReference objectReference, string[] path)
        {
            //TODO replace most of this with getObjectAtPath(path.take(-1))
            object root = UnderlyingDict;
            for (int i = 0; i < path.Length - 1; i++)
            {
                string part = path[i];

                if (root is PdfDictionary)
                {
                    PdfDictionary temp = (PdfDictionary)root;
                    if (temp.ContainsKey(part))
                    {
                        root = temp.Get<object>(part);
                    }
                    else
                    {
                        throw new NotImplementedException();
                    }
                }
                else if (root is PdfList)
                {
                    int index;
                    if (int.TryParse(part, out index) == false)
                    {
                        throw new NotImplementedException();
                    }

                    PdfList temp = (PdfList)root;
                    if (temp.Count <= index)
                    {
                        throw new NotImplementedException();
                    }

                    root = temp.Get<object>(index);
                }

                if (root is ObjectReference)
                {
                    string[] subPath = path.Take(i + 1).ToArray();
                    root = GetObjectAtPath(subPath);
                }
            }

            if (root is PdfDictionary)
            {
                PdfDictionary temp = (PdfDictionary)root;
                temp.Set(path.Last(), objectReference);
            }
            else if (root is PdfList)
            {
                int index;
                if (int.TryParse(path.Last(), out index) == false)
                {
                    throw new NotImplementedException();
                }

                PdfList temp = (PdfList)root;
                if (temp.Count <= index)
                {
                    throw new NotImplementedException();
                }

                temp.Set(index, objectReference);
            }
        }

        /// <summary>
        /// returns all form xObjects found in the XObject dictionary
        /// does not return form xObjects
        /// </summary>
        public IEnumerable<Name> ListXObjectImageNames()
        {
            PdfDictionary xObjects = (PdfDictionary)GetObjectAtPath("XObject");
            if (xObjects == null)
            {
                yield break;
            }

            foreach (Name key in xObjects.Keys)
            {
                object xObject = xObjects.Get<object>(key);
                if (xObject is XObjectImage)
                {
                    yield return key;
                }
            }
        }

        /// <summary>
        /// returns the xObject image with the given name, or null if it cannot be found or is not an image
        /// </summary>
        public XObjectImage GetXObjectImage(string xObjectName)
        {
            object xObject = GetObjectAtPath("XObject", xObjectName);
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
        public XObjectForm GetXObjectForm(string xObjectName)
        {
            object xObject = GetObjectAtPath("XObject", xObjectName);
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
