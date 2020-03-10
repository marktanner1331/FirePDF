using FirePDF.Model;
using FirePDF.Reading;
using FirePDF.Writing;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace FirePDF
{
    public class PDF : IHaveUnderlyingDict, IDisposable, IEnumerable<Page>
    {
        internal ObjectStore store;

        //todo change this to a dictionary accessor
        internal Catalog catalog;

        public readonly float version;
        internal long lastXrefOffset;

        public PDF(string fullFilePath) : this(File.OpenRead(fullFilePath)) { }

        public PDF(Stream stream)
        {
            this.pdf = this;
            this.version = PDFReader.readVersion(stream);

            lastXrefOffset = PDFReader.findLastXREFOffset(stream);
            stream.Position = lastXrefOffset;

            ObjectReference root;
            XREFTable table;
            PDFReader.readXREFTables(this, stream, out table, out root);

            this.store = new ObjectStore(this, table, stream);
            this.underlyingDict = new PDFDictionary(this);

            //do this better
            //need to preserve the trailer in readPDF
            this.underlyingDict.set("Root", root);

            this.catalog = root.get<Catalog>();
        }

        public PDF()
        {
            this.pdf = this;

            //need to initialize this with the underlying dict from the new Catalog()
            this.underlyingDict = new PDFDictionary(this);

            this.store = new ObjectStore(this);
            this.version = 1.4f;

            this.catalog = new Catalog(this);
            this.underlyingDict.set("Root", store.add(catalog.underlyingDict));
        }

        /// <summary>
        /// searches the pdf for all objects that are of the given type and returns them
        /// </summary>
        public List<T> getAll<T>()
        {
            return store.getAll<T>();
        }

        /// <summary>
        /// Gets an object from the pdf and casts it to the given type.
        /// This method crashes if the object canoot be found
        /// </summary>
        /// <typeparam name="T">The type to cast the object to, to leave it uncasted, pass through 'object' as the type</typeparam>
        /// <param name="objectNumber">The number of the object reference</param>
        /// <param name="generation">the generation of the object reference</param>
        /// <returns>The object, casted to the given type</returns>
        public T get<T>(int objectNumber, int generation)
        {
            return store.get<T>(objectNumber, generation);
        }

        /// <summary>
        /// Gets an object from the pdf and casts it to the given type.
        /// This method crashes if the object canoot be found
        /// </summary>
        /// <typeparam name="T">The type to cast the object to, to leave it uncasted, pass through 'object' as the type</typeparam>
        /// <param name="indirectReference"></param>
        /// <returns>The object, casted to the given type</returns>
        public T get<T>(ObjectReference indirectReference)
        {
            return store.get<T>(indirectReference);
        }

        public ObjectReference addStream(string data)
        {
            using (MemoryStream ms = new MemoryStream())
            using (StreamWriter writer = new StreamWriter(ms))
            {
                writer.Write(data);
                writer.Flush();
                ms.Seek(0, SeekOrigin.Begin);
                return addStream(ms);
            }
        }

        public ObjectReference addStream(byte[] data)
        {
            using (MemoryStream ms = new MemoryStream(data))
            {
                ms.Seek(0, SeekOrigin.Begin);
                return addStream(ms);
            }
        }

        public ObjectReference addStream(FileInfo file)
        {
            using (Stream stream = File.OpenRead(file.FullName))
            {
                return addStream(stream);
            }
        }

        public ObjectReference addStream(Stream stream)
        {
            //TODO have one global newStream, backed by a file maybe?
            //instead of lots of little memory streams hanging around

            MemoryStream ms = new MemoryStream();
            stream.CopyTo(ms);
            ms.Seek(0, SeekOrigin.Begin);

            PDFDictionary streamDict = new PDFDictionary(pdf);
            streamDict.set("Length", (int)ms.Length);

            return store.addStream(ms, streamDict);
        }

        public void save(string fullFilePath, SaveType saveType = SaveType.Fresh)
        {
            if (saveType == SaveType.Fresh)
            {
                PDFWriter writer = new PDFWriter(File.Create(fullFilePath), false);
                writer.writeNewPDF(this);
            }
            else
            {
                using (Stream stream = File.Create(fullFilePath))
                {
                    PDFWriter writer = new PDFWriter(stream, false);
                    store.copyExistingStream(stream);
                    writer.writeUpdatedPDF(this);
                }
            }
        }

        public Page addPage(Page page)
        {
            return insertPage(page, catalog.numPages() + 1);
        }

        public Page insertPage(Page page, int oneBasedPageNumber)
        {
            if (page.pdf != this)
            {
                ObjectReference objRef = store.add(page);
                Page newPage = objRef.get<Page>();
                catalog.insertPage(newPage, objRef, oneBasedPageNumber);
                return newPage;
            }
            else
            {
                if (page.isOrphan)
                {
                    ObjectReference objRef = store.add(page);
                    catalog.insertPage(page, objRef, oneBasedPageNumber);
                    page.isOrphan = false;
                    return page;
                }
                else
                {
                    ObjectReference objRef = store.add(page);
                    Page newPage = objRef.get<Page>();
                    catalog.insertPage(newPage, objRef, oneBasedPageNumber);
                    return newPage;
                }
            }
        }

        public Page getPage(int oneBasedPageNumber)
        {
            return catalog.getPage(oneBasedPageNumber);
        }

        /// <summary>
        /// returns the number of pages in the pdf
        /// </summary>
        public int numPages()
        {
            return catalog.numPages();
        }

        public IEnumerator<Page> GetEnumerator() => catalog.GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => catalog.GetEnumerator();

        public void Dispose()
        {
            store.Dispose();
        }
    }
}
