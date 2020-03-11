using FirePDF.Reading;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;

namespace FirePDF.Model
{
    /// <summary>
    /// this class manages reading, caching, and writing objects to the various Pdf streams
    /// </summary>
    public class ObjectStore : IDisposable
    {
        //objects can either be stored in the existing table or the new table
        //if we are reading an existing Pdf then everything will be in the existing table
        //but if we are adding new bits to it, or constructing a Pdf from scratch then it will be in the newTable
        //object numbers will be unique, i.e. you wont find the same number in both tables
        
        private readonly Pdf pdf;
        
        private readonly XrefTable existingTable;
        private readonly Stream existingStream;

        private readonly XrefTable newTable;
        private readonly Stream newStream;

        private readonly Dictionary<ObjectReference, object> cache;
        
        /// <summary>
        /// maps objects stored in other pdf's to object references in this Pdf
        /// in the case that we are adding the same object from the same Pdf twice
        /// we don't want to duplicate the object, so this map stops that
        /// </summary>
        private readonly Dictionary<Pdf, Dictionary<ObjectReference, ObjectReference>> externalObjectMap;

        public ObjectStore(Pdf pdf, XrefTable existingTable, Stream existingStream)
        {
            this.pdf = pdf;
            this.existingTable = existingTable;
            this.existingStream = existingStream;
            newTable = new XrefTable();
            newTable.MergeIn(existingTable);
            newStream = new MemoryStream();
            cache = new Dictionary<ObjectReference, object>();
            externalObjectMap = new Dictionary<Pdf, Dictionary<ObjectReference, ObjectReference>>();
        }

        /// <summary>
        /// searches for all objects that are of the given type and returns them
        /// </summary>
        internal List<T> GetAll<T>()
        {
            List<T> objects = new List<T>();

            //hint: the newTable contains both the existing and new records
            foreach (XrefTable.XrefRecord record in newTable.GetAllXrefRecords())
            {
                object obj = Get<object>(record.objectNumber, record.generation);
                if (obj is T o)
                {
                    objects.Add(o);
                }
            }

            return objects;
        }

        /// <summary>
        /// if the Pdf is from an existing file then this method will copy the existing data over to a new stream
        /// </summary>
        /// <param name="outStream">the stream to copy the data to</param>
        public void CopyExistingStream(Stream outStream)
        {
            existingStream.Position = 0;
            existingStream.CopyTo(outStream);
        }

        public bool IsExistingObject(ObjectReference indirectReference)
        {
            return existingTable.HasXrefRecord(indirectReference);
        }

        //it is ok for the existingStream to be null
        //because if nothing is in the existingTable then we shouldn't even try to look it up in the stream
        public ObjectStore(Pdf pdf) : this(pdf, new XrefTable(), null) { }

        public T Get<T>(int objectNumber, int generation) => Get<T>(new ObjectReference(pdf, objectNumber, generation));
        
        public T Get<T>(ObjectReference indirectReference)
        {
            //Debug.WriteLine("Getting object: " + indirectReference);

            if(cache.ContainsKey(indirectReference))
            {
                return (T)cache[indirectReference];
            }

            XrefTable.XrefRecord? record = existingTable.GetXrefRecord(indirectReference.objectNumber, indirectReference.generation);
            T obj;
            
            if (record != null)
            {
                //var k = PDFReader.readIndirectObject(Pdf, existingStream, record.Value);
                obj = (T)PdfReader.ReadIndirectObject(pdf, existingStream, record.Value);
            }
            else
            {
                //this will only really happen if a new object is not in the cache
                //so large objects such as images
                //or if we have had to clear the cache for whatever reason
                record = newTable.GetXrefRecord(indirectReference.objectNumber, indirectReference.generation);
                obj = (T)PdfReader.ReadIndirectObject(pdf, newStream, record.Value);
            }

            cache[indirectReference] = obj;
            return obj;
        }

        /// <summary>
        /// returns the object reference for the given object, or null if it cannot be found
        /// </summary>
        internal ObjectReference ReverseGet(object value)
        {
            return cache.FirstOrDefault(x => x.Value == value).Key;
        }

        private ObjectReference AddNewObject(object obj)
        {
            int nextNumber = newTable.GetNextFreeRecordNumber();
            if(nextNumber == 6)
            {

            }

            newTable.AddRecord(new XrefTable.XrefRecord
            {
                objectNumber = nextNumber,
                generation = 0,
                offset = 0
            });

            ObjectReference objRef = new ObjectReference(pdf, nextNumber, 0);

            cache.Add(objRef, obj);

            return objRef;
        }

        public ObjectReference AddStream(Stream stream, PdfDictionary streamDictionary)
        {
            long position = newStream.Length;
            newStream.Position = newStream.Length;

            stream.CopyTo(newStream);

            return AddNewObject(PdfStream.FromDictionary(streamDictionary, newStream, position));
        }

        public ObjectReference Add(object obj)
        {
            if(obj is HavePdf && (obj as HavePdf).Pdf != pdf)
            {
                if(obj is ObjectReference)
                {
                    ObjectReference objRef = obj as ObjectReference;
                    if(externalObjectMap.ContainsKey(objRef.Pdf) == false)
                    {
                        externalObjectMap.Add(objRef.Pdf, new Dictionary<ObjectReference, ObjectReference>());
                    }

                    if (externalObjectMap[objRef.Pdf].ContainsKey(objRef) == false)
                    {
                        externalObjectMap[objRef.Pdf].Add(objRef, AddNewObject(DeepClone(objRef.Get<object>())));
                    }

                    return externalObjectMap[objRef.Pdf][objRef];
                }
                else
                {
                    return AddNewObject(DeepClone(obj));
                }
            }
            else
            {
                return AddNewObject(obj);
            }
        }

        private object ShallowClone(object obj)
        {
            switch (obj)
            {
                case PdfDictionary dict:
                {
                    Dictionary<Name, object> newDict = new Dictionary<Name, object>();

                    foreach (string key in dict.Keys)
                    {
                        //we always want to copy resources as a separate object
                        //so that we don't end up with two pages pointing to the same resources
                        if(key == "Resources" && dict.ContainsKey("Type") && (dict.Get<Name>("Type") == "Page" || dict.Get<Name>("Type") == "XObject"))
                        {
                            newDict.Add("Resources", Add(ShallowClone(dict.Get<PdfDictionary>("Resources"))));
                        }
                        else
                        {
                            object value = dict.Get<object>(key, false);
                            newDict.Add(key, value);
                        }
                    }

                    return new PdfDictionary(pdf, newDict);
                }
                case PdfList list:
                    return new PdfList(pdf, list.Cast<object>(false));
                case ObjectReference reference:
                    //shallow clone will resolve object references and return the resolved object
                    //its a bit squiffy as it means shallowClone will return an object of a different type 
                    //to that which was passed in
                    //but if you think about it, object references are just pointers
                    //they take on the type that they point to
                    //so we aren't really changing type, but dereferencing it instead
                    return reference.Get<object>();
                case HaveUnderlyingDict underlyingDict:
                {
                    PdfDictionary dict = underlyingDict.UnderlyingDict;
                    dict = (PdfDictionary)DeepClone(dict);
                    return HaveUnderlyingDict.FromDictionary(dict);
                }
                default:
                    return obj;
            }
        }

        private object DeepClone(object obj)
        {
            switch (obj)
            {
                case PdfDictionary dict:
                {
                    Dictionary<Name, object> newDict = new Dictionary<Name, object>();

                    foreach(string key in dict.Keys)
                    {
                        //we always want to copy resources as a separate object
                        //so that we don't end up with two pages pointing to the same resources
                        if (key == "Resources" && dict.ContainsKey("Type") && (dict.Get<Name>("Type") == "Page" || dict.Get<Name>("Type") == "XObject"))
                        {
                            //by resolving the reference here bypass the external object cache
                            newDict.Add("Resources", Add(DeepClone(dict.Get<PdfDictionary>("Resources"))));
                        }
                        else if(key == "Parent" && dict.ContainsKey("Type") && dict.Get<Name>("Type") == "Page")
                        {
                            continue;
                        }
                        else
                        {
                            object value = dict.Get<object>(key, false);
                            newDict.Add(key, DeepClone(value));
                        }
                    }

                    return new PdfDictionary(pdf, newDict);
                }
                case PdfList list:
                {
                    List<object> temp = list.Select(DeepClone).ToList();

                    return new PdfList(pdf, temp);
                }
                case ObjectReference reference:
                {
                    ObjectReference objRef = reference;
                    return Add(objRef);
                }
                case PdfStream pdfStream:
                {
                    PdfStream stream = pdfStream;
                    PdfDictionary dict = (PdfDictionary)DeepClone(stream.UnderlyingDict);

                    using (Stream compressedStream = stream.GetCompressedStream())
                    {
                        long position = newStream.Length;

                        newStream.Position = newStream.Length;
                        compressedStream.CopyTo(newStream);

                        return PdfStream.FromDictionary(dict, newStream, position);
                    }
                }
                case HaveUnderlyingDict underlyingDict:
                {
                    PdfDictionary dict = underlyingDict.UnderlyingDict;
                    dict = (PdfDictionary)DeepClone(dict);
                    return HaveUnderlyingDict.FromDictionary(dict);
                }
                default:
                    return obj;
            }
        }

        public void Dispose()
        {
            existingStream?.Dispose();
            existingStream?.Close();
            newStream?.Dispose();
            newStream?.Close();
        }
    }
}
