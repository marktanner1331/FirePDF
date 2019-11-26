using FirePDF.Reading;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FirePDF.Model
{
    /// <summary>
    /// this class manages reading, caching, and writing objects to the various pdf streams
    /// </summary>
    //TODO when saving
    //  we will need to load of a dictionaries values
    //  even if they are not cached
    //  one of their children might be dirty
    //  so we will need to load it
    //  to check its children
    //  and then set its new object reference
    //  if necessary 
    //  then it will be dirty
    //  so we serialize it
    //   i.e. hte modification bubbles up
    //  but then if we try to write it again we don't want to do a deep write
    //  but we can't make the original undirty
    //  other other things still reference it
    //  so what we need is a map from old dirty objects to their clean counterparts
    //  note that this doesnt matter when simply reading/copying from other object stores
    //  as we are simply resolving the object reference of teh dirty object
    //  and that will return the dirty object, which is fine
    //  we aren't actually modifying the object references when reading
    public class ObjectStore
    {
        //objects can either be stored in the existing table or the new table
        //if we are reading an existing pdf then everything will be in the existing table
        //but if we are adding new bits to it, or constructing a pdf from scratch then it will be in the newTable
        //object numbers will be unique, i.e. you wont find the same number in both tables
        
        private PDF pdf;
        
        private XREFTable existingTable;
        private Stream existingStream;

        private XREFTable newTable;
        private Stream newStream;

        private Dictionary<ObjectReference, object> cache;
        
        /// <summary>
        /// maps objects stored in other pdf's to object references in this pdf
        /// in the case that we are adding the same object from the same pdf twice
        /// we don't want to duplicate the object, so this map stops that
        /// </summary>
        private Dictionary<PDF, Dictionary<ObjectReference, ObjectReference>> externalObjectMap;

        public ObjectStore(PDF pdf, XREFTable existingTable, Stream existingStream)
        {
            this.pdf = pdf;
            this.existingTable = existingTable;
            this.existingStream = existingStream;
            this.newTable = new XREFTable();
            this.newTable.mergeIn(existingTable);
            this.newStream = new MemoryStream();
            this.cache = new Dictionary<ObjectReference, object>();
            this.externalObjectMap = new Dictionary<PDF, Dictionary<ObjectReference, ObjectReference>>();
        }

        public bool isExistingObject(ObjectReference indirectReference)
        {
            return existingTable.hasXREFRecord(indirectReference);
        }

        //it is ok for the existingStream to be null
        //because if nothing is in the existingTable then we shouldn't even try to look it up in the stream
        public ObjectStore(PDF pdf) : this(pdf, new XREFTable(), null) { }

        public T get<T>(int objectNumber, int generation) => get<T>(new ObjectReference(pdf, objectNumber, generation));
        
        public T get<T>(ObjectReference indirectReference)
        {
            Debug.WriteLine("Getting object: " + indirectReference);

            if(cache.ContainsKey(indirectReference))
            {
                return (T)cache[indirectReference];
            }

            XREFTable.XREFRecord? record = existingTable.getXREFRecord(indirectReference.objectNumber, indirectReference.generation);
            T obj;
            
            if (record != null)
            {
                //var k = PDFReader.readIndirectObject(pdf, existingStream, record.Value);
                obj = (T)PDFReader.readIndirectObject(pdf, existingStream, record.Value);
            }
            else
            {
                //this will only really happen if a new object is not in the cache
                //so large objects such as images
                //or if we have had to clear the cache for whatever reason
                record = newTable.getXREFRecord(indirectReference.objectNumber, indirectReference.generation);
                obj = (T)PDFReader.readIndirectObject(pdf, newStream, record.Value);
            }

            cache[indirectReference] = obj;
            return obj;
        }

        /// <summary>
        /// returns the object reference for the given ubject, or null if it cannot be found
        /// </summary>
        internal ObjectReference reverseGet(object value)
        {
            return cache.Where(X => X.Value == value).FirstOrDefault().Key;
        }

        private ObjectReference addNewObject(object obj)
        {
            int nextNumber = newTable.getNextFreeRecordNumber();
            newTable.addRecord(new XREFTable.XREFRecord
            {
                objectNumber = nextNumber,
                generation = 0,
                offset = 0
            });

            ObjectReference objRef = new ObjectReference(pdf, nextNumber, 0);

            cache.Add(objRef, obj);

            return objRef;
        }

        public ObjectReference add(object obj, bool checkCache = true)
        {
            if(obj is IHavePDF && (obj as IHavePDF).pdf != pdf)
            {
                if(obj is ObjectReference)
                {
                    ObjectReference objRef = obj as ObjectReference;
                    if(externalObjectMap.ContainsKey(objRef.pdf) == false)
                    {
                        externalObjectMap.Add(objRef.pdf, new Dictionary<ObjectReference, ObjectReference>());
                    }

                    if (externalObjectMap[objRef.pdf].ContainsKey(objRef) == false)
                    {
                        externalObjectMap[objRef.pdf].Add(objRef, addNewObject(deepClone(objRef.get<object>())));
                    }

                    return externalObjectMap[objRef.pdf][objRef];
                }
                else
                {
                    return addNewObject(deepClone(obj));
                }
            }
            else
            {
                return addNewObject(obj);
            }
        }

        private object shallowClone(object obj)
        {
            if (obj is PDFDictionary)
            {
                PDFDictionary dict = obj as PDFDictionary;
                Dictionary<Name, object> newDict = new Dictionary<Name, object>();

                foreach (string key in dict.keys)
                {
                    //we always want to copy resources as a seperate object
                    //so that we don't end up with two pages pointing to the same resources
                    if(key == "Resources" && dict.ContainsKey("Type") && (dict.get<Name>("Type") == "Page" || dict.get<Name>("Type") == "XObject"))
                    {
                        newDict.Add("Resources", add(dict.get<PDFDictionary>("Resources")));
                    }
                    else
                    {
                        object value = dict.get<object>(key, false);
                        newDict.Add(key, value);
                    }
                }

                return new PDFDictionary(pdf, newDict);
            }
            else if (obj is PDFList)
            {
                return new PDFList(pdf, (obj as PDFList).cast<object>(false));
            }
            else if(obj is ObjectReference)
            {
                //shallow clone will resolve object references and return the resolved object
                //its a bit squiffy as it means shallowClone will return an object of a different type 
                //to that which was passed in
                //but if you think about it, object references are just pointers
                //they take on the type that they point to
                //so we aren't really changing type, but dereferencing it instead
                return (obj as ObjectReference).get<object>();
            }
            else if(obj is IHaveUnderlyingDict)
            {
                PDFDictionary dict = ((IHaveUnderlyingDict)obj).underlyingDict;
                dict = (PDFDictionary)deepClone(dict);
                return IHaveUnderlyingDict.fromDictionary(dict);
            }
            else
            {
                return obj;
            }
        }

        private object deepClone(object obj)
        {
            if(obj is PDFDictionary)
            {
                PDFDictionary dict = obj as PDFDictionary;
                Dictionary<Name, object> newDict = new Dictionary<Name, object>();

                foreach(string key in dict.keys)
                {
                    //we always want to copy resources as a seperate object
                    //so that we don't end up with two pages pointing to the same resources
                    if (key == "Resources" && dict.ContainsKey("Type") && (dict.get<Name>("Type") == "Page" || dict.get<Name>("Type") == "XObject"))
                    {
                        //by resolving the reference here bypass the external object cache
                        newDict.Add("Resources", add(dict.get<PDFDictionary>("Resources")));
                    }
                    else
                    {
                        object value = dict.get<object>(key, false);
                        newDict.Add(key, deepClone(value));
                    }
                }

                return new PDFDictionary(pdf, newDict);
            }
            else if(obj is PDFList)
            {
                return new PDFList(pdf, (obj as PDFList).cast<object>(false));
            }
            else if(obj is ObjectReference)
            {
                ObjectReference objRef = obj as ObjectReference;
                return add(objRef);
            }
            else if(obj is PDFStream)
            {
                PDFStream stream = obj as PDFStream;
                PDFDictionary dict = (PDFDictionary)deepClone(stream.underlyingDict);

                using (Stream compressedStream = stream.getCompressedStream())
                {
                    long position = newStream.Length;

                    newStream.Position = newStream.Length;
                    compressedStream.CopyTo(newStream);

                    return PDFStream.fromDictionary(dict, newStream, position);
                }
            }
            else if(obj is IHaveUnderlyingDict)
            {
                PDFDictionary dict = ((IHaveUnderlyingDict)obj).underlyingDict;
                dict = (PDFDictionary)deepClone(dict);
                return IHaveUnderlyingDict.fromDictionary(dict);
            }
            else
            {
                return obj;
            }
        }

        public bool isDirty(ObjectReference objectReference)
        {
            HashSet<ObjectReference> completed = new HashSet<ObjectReference>();

            Queue<ObjectReference> queue = new Queue<ObjectReference>();
            queue.Enqueue(objectReference);

            while(queue.Count > 0)
            {
                ObjectReference objectRef = queue.Dequeue();

                if (completed.Contains(objectRef))
                {
                    continue;
                }

                completed.Add(objectRef);

                object value = objectRef.get<object>();
                
                if (value is ICanBeDirty)
                {
                    if((value as ICanBeDirty).isDirtyShallow())
                    {
                        return true;
                    }

                    List<ObjectReference> newReferences = (value as IHaveChildren).GetObjectReferences().ToList();
                    foreach (ObjectReference newReference in newReferences)
                    {
                        queue.Enqueue(newReference);
                    }
                }
            }

            return false;
        }
    }
}
