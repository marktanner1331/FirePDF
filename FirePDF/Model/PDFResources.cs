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
        /// adds the contents of the given resources
        /// the names of the resources are renamed to avoid collisions
        /// a dictionary is returned as a map from old to new
        /// this dictionary contains fully qualified paths, e.g. XObject/form1
        /// </summary>
        public Dictionary<string, string> MergeResources(PdfResources other)
        {
            //untested!!

            Dictionary<string, string> map = new Dictionary<string, string>();

            foreach (string key in other.UnderlyingDict.Keys)
            {
                if (UnderlyingDict.ContainsKey(key) == false)
                {
                    UnderlyingDict.Set(key, other.UnderlyingDict.Get(key, false));

                    foreach (string path in enumerateSubResourcePaths(this, key))
                    {
                        //these are brand new resources
                        //so we can keep the original names
                        map.Add(path, path);
                    }
                }
                else if (UnderlyingDict.Get(key, true) is PdfList sourceList && other.UnderlyingDict.Get(key, true) is PdfList destList)
                {
                    //TODO check it can be merged before actually merging

                    //this can be tricky
                    //we really need these lists to match
                    //don't really have much choice if they don't
                    if (Enumerable.SequenceEqual(sourceList, destList) == false)
                    {
                        throw new Exception("Cannot merge because: " + key + " are lists which are not equal");
                    }

                    foreach (string path in enumerateSubResourcePaths(this, key))
                    {
                        //we can keep the original names
                        map.Add(path, path);
                    }
                }
                else if (UnderlyingDict.Get(key, true) is PdfDictionary sourceDict && other.UnderlyingDict.Get(key, true) is PdfDictionary destDict)
                {
                    foreach (KeyValuePair<Name, object> entry in destDict)
                    {
                        if (entry.Value is ObjectReference && sourceDict.ContainsValue(entry.Value, out Name existingKey))
                        {
                            //we have already got an entry for this value
                            map.Add(key + "/" + entry.Key, key + "/" + existingKey);
                        }
                        else
                        {
                            //TODO
                            //maybe in future we can check for existing direct values
                            //that are identical, and reuse those
                            //this happens sometimes with ExtGStates

                            if (sourceDict.ContainsKey(entry.Key) == false)
                            {
                                //no existing key with that name, great
                                sourceDict.Set(entry.Key, entry.Value);

                                //no renaming necessary here
                                map.Add(key + "/" + entry.Key, key + "/" + entry.Key);
                            }
                            else
                            {
                                //we already have a key with that name
                                String tempKey = entry.Key;
                                int i = 1;
                                while (sourceDict.ContainsKey(tempKey + "_" + i))
                                {
                                    i++;
                                }

                                tempKey += "_" + i;

                                sourceDict.Set(tempKey, entry.Value);
                                map.Add(key + "/" + entry.Key, key + "/" + tempKey);
                            }
                        }
                    }
                }
                else
                {
                    throw new Exception("underlying types are not of the same type");
                }
            }

            return map;
        }

        public void removeResource(string type, string name)
        {
            //TODO: change 'type' into an enum

            PdfDictionary dict = (PdfDictionary)GetObjectAtPath(type);
            if (dict == null)
            {
                return;
            }

            dict.RemoveEntry(name);
        }

        /// <summary>
        /// returns a new resource name that is unique at the specified path
        /// </summary>
        /// <remarks>
        /// Used primarily for creating new resources. I.e. to insert a new ExtGState, call generateNewResourceName("ExtGState") to generate it's name
        /// </remarks>
        public Name generateNewResourceName(string path)
        {
            if (path.Contains("/"))
            {
                throw new ArgumentException("path cannot contain /'s");
            }

            if (UnderlyingDict.ContainsKey(path) == false)
            {
                return "1";
            }

            object value = UnderlyingDict.Get(path, true);
            if (value is PdfDictionary == false)
            {
                throw new Exception("Can only generate resource names for dictionaries");
            }

            PdfDictionary dict = value as PdfDictionary;

            int i = 1;
            while (dict.ContainsKey(i.ToString()))
            {
                i++;
            }

            return i.ToString();
        }

        /// <summary>
        /// enumerates all the resource paths for a given key in the given PDFResources
        /// </summary>
        private static IEnumerable<string> enumerateSubResourcePaths(PdfResources resources, string key)
        {
            //untested!!

            object value = resources.UnderlyingDict.Get(key, true);
            if (value is PdfDictionary dict)
            {
                foreach (string key2 in dict.Keys)
                {
                    yield return key + "/" + key2;
                }
            }
            else if (value is PdfList list)
            {
                for (int i = 0; i < list.Count; i++)
                {
                    yield return key + "/" + i;
                }
            }
            else
            {
                throw new NotImplementedException("unknown resource underlying type: " + value.GetType().Name);
            }
        }

        public IEnumerable<string> enumerateResourcePaths()
        {
            //untested!!

            foreach (string key in UnderlyingDict.Keys)
            {
                foreach (string path in enumerateSubResourcePaths(this, key))
                {
                    yield return path;
                }
            }
        }

        public IEnumerable<ExtGState> GetAllExtGStates()
        {
            PdfDictionary extGStates = (PdfDictionary)GetObjectAtPath("ExtGState");
            if (extGStates == null)
            {
                yield break;
            }

            foreach (KeyValuePair<Name, object> entry in extGStates)
            {
                object obj = entry.Value;
                if (obj is ObjectReference)
                {
                    obj = (obj as ObjectReference).Get<object>();
                }

                if (obj is ExtGState state)
                {
                    yield return state;
                }
                else if (obj is PdfDictionary dict)
                {
                    yield return new ExtGState(dict);
                }
                else
                {
                    throw new Exception();
                }
            }
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

        public IEnumerable<ObjectReference> ListImages(bool recursive)
        {
            List<ObjectReference> images = new List<ObjectReference>();

            PdfDictionary xObjects = (PdfDictionary)GetObjectAtPath("XObject");
            if (xObjects == null)
            {
                return images;
            }

            foreach (KeyValuePair<Name, object> entry in xObjects)
            {
                ObjectReference objRef = entry.Value as ObjectReference;
                if (objRef.Get<object>() is XObjectImage)
                {
                    images.Add(objRef);
                }
                else if (recursive && objRef.Get<object>() is XObjectForm form)
                {
                    images.AddRange(form.Resources.ListImages(true));
                }
            }

            return images;
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
        /// does not resolve any indirect references
        /// TODO used combinedPaths, and send the boolean through instead of two methods
        /// </summary>
        public object GetUnresolvedObjectAtPath(params string[] path)
        {
            //TODO make this a generic method like getObjectAtPath<Font>()?

            object root = UnderlyingDict;

            int count = 0;
            foreach (string part in path)
            {
                bool isLast = ++count == path.Length;

                switch (root)
                {
                    case PdfDictionary dictionary when dictionary.ContainsKey(part):
                        root = dictionary.Get(part, !isLast);
                        break;
                    case PdfDictionary dictionary:
                        return null;
                    case PdfResources resources:
                        root = resources.UnderlyingDict.Get(part, !isLast);
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

                            root = temp.Get<object>(index, !isLast);
                            break;
                        }
                }
            }

            return root;
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
                        root = dictionary.Get(part, true);
                        break;
                    case PdfDictionary dictionary:
                        return null;
                    case PdfResources resources:
                        root = resources.UnderlyingDict.Get(part, true);
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

                            root = temp.Get<object>(index, true);
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

        public void SetObjectAtPath(object obj, params string[] path)
        {
            //TODO replace most of this with getObjectAtPath(path.take(-1))
            //TODO also need to create the object if it doesn't exist

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
                        temp.Set(part, new PdfDictionary(Pdf));
                        root = temp.Get<object>(part);
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
                temp.Set(path.Last(), obj);
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

                temp.Set(index, obj);
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
