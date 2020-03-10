using FirePDF.Model;
using FirePDF.Reading;
using FirePDF.Writing;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;

namespace FirePDF
{
    public class Pdf : HaveUnderlyingDict, IDisposable, IEnumerable<Page>
    {
        internal ObjectStore store;

        //todo change this to a dictionary accessor
        internal Catalog catalog;

        public readonly float version;
        internal long lastXrefOffset;

        public Pdf(string fullFilePath) : this(File.OpenRead(fullFilePath)) { }

        public Pdf(Stream stream)
        {
            Pdf = this;
            version = PdfReader.ReadVersion(stream);

            lastXrefOffset = PdfReader.FindLastXrefOffset(stream);
            stream.Position = lastXrefOffset;

            ObjectReference root;
            XrefTable table;
            PdfReader.ReadXrefTables(this, stream, out table, out root);

            store = new ObjectStore(this, table, stream);
            UnderlyingDict = new PdfDictionary(this);

            //do this better
            //need to preserve the trailer in readPDF
            UnderlyingDict.Set("Root", root);

            catalog = root.Get<Catalog>();
        }

        public Pdf()
        {
            Pdf = this;

            //need to initialize this with the underlying dict from the new Catalog()
            UnderlyingDict = new PdfDictionary(this);

            store = new ObjectStore(this);
            version = 1.4f;

            catalog = new Catalog(this);
            UnderlyingDict.Set("Root", store.Add(catalog.UnderlyingDict));
        }

        /// <summary>
        /// searches the Pdf for all objects that are of the given type and returns them
        /// </summary>
        public List<T> GetAll<T>()
        {
            return store.GetAll<T>();
        }

        /// <summary>
        /// Gets an object from the Pdf and casts it to the given type.
        /// This method crashes if the object cannot be found
        /// </summary>
        /// <typeparam name="T">The type to cast the object to, to leave it uncasted, pass through 'object' as the type</typeparam>
        /// <param name="objectNumber">The number of the object reference</param>
        /// <param name="generation">the generation of the object reference</param>
        /// <returns>The object, casted to the given type</returns>
        public T Get<T>(int objectNumber, int generation)
        {
            return store.Get<T>(objectNumber, generation);
        }

        /// <summary>
        /// Gets an object from the Pdf and casts it to the given type.
        /// This method crashes if the object cannot be found
        /// </summary>
        /// <typeparam name="T">The type to cast the object to, to leave it uncasted, pass through 'object' as the type</typeparam>
        /// <param name="indirectReference"></param>
        /// <returns>The object, casted to the given type</returns>
        public T Get<T>(ObjectReference indirectReference)
        {
            return store.Get<T>(indirectReference);
        }

        public ObjectReference AddStream(string data)
        {
            using (MemoryStream ms = new MemoryStream())
            using (StreamWriter writer = new StreamWriter(ms))
            {
                writer.Write(data);
                writer.Flush();
                ms.Seek(0, SeekOrigin.Begin);
                return AddStream(ms);
            }
        }

        public ObjectReference AddStream(byte[] data)
        {
            using (MemoryStream ms = new MemoryStream(data))
            {
                ms.Seek(0, SeekOrigin.Begin);
                return AddStream(ms);
            }
        }

        public ObjectReference AddStream(FileInfo file)
        {
            using (Stream stream = File.OpenRead(file.FullName))
            {
                return AddStream(stream);
            }
        }

        public ObjectReference AddStream(Stream stream)
        {
            //TODO have one global newStream, backed by a file maybe?
            //instead of lots of little memory streams hanging around

            MemoryStream ms = new MemoryStream();
            stream.CopyTo(ms);
            ms.Seek(0, SeekOrigin.Begin);

            PdfDictionary streamDict = new PdfDictionary(Pdf);
            streamDict.Set("Length", (int)ms.Length);

            return store.AddStream(ms, streamDict);
        }

        public void Save(string fullFilePath, SaveType saveType = SaveType.Fresh)
        {
            if (saveType == SaveType.Fresh)
            {
                PdfWriter writer = new PdfWriter(File.Create(fullFilePath), false);
                writer.WriteNewPdf(this);
            }
            else
            {
                using (Stream stream = File.Create(fullFilePath))
                {
                    PdfWriter writer = new PdfWriter(stream, false);
                    store.CopyExistingStream(stream);
                    writer.WriteUpdatedPdf(this);
                }
            }
        }

        public Page AddPage(Page page)
        {
            return InsertPage(page, catalog.NumPages() + 1);
        }

        public Page InsertPage(Page page, int oneBasedPageNumber)
        {
            if (page.Pdf != this)
            {
                ObjectReference objRef = store.Add(page);
                Page newPage = objRef.Get<Page>();
                catalog.InsertPage(newPage, objRef, oneBasedPageNumber);
                return newPage;
            }
            else
            {
                if (page.IsOrphan)
                {
                    ObjectReference objRef = store.Add(page);
                    catalog.InsertPage(page, objRef, oneBasedPageNumber);
                    page.IsOrphan = false;
                    return page;
                }
                else
                {
                    ObjectReference objRef = store.Add(page);
                    Page newPage = objRef.Get<Page>();
                    catalog.InsertPage(newPage, objRef, oneBasedPageNumber);
                    return newPage;
                }
            }
        }

        public Page GetPage(int oneBasedPageNumber)
        {
            return catalog.GetPage(oneBasedPageNumber);
        }

        /// <summary>
        /// returns the number of pages in the Pdf
        /// </summary>
        public int NumPages()
        {
            return catalog.NumPages();
        }

        public IEnumerator<Page> GetEnumerator() => catalog.GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => catalog.GetEnumerator();

        public void Dispose()
        {
            store.Dispose();
        }
    }
}
