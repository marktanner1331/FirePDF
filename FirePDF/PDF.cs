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
    public class PDF : IEnumerable<Page>
    {
        internal ObjectStore store;
        internal Catalog catalog;
        
        public readonly float version;

        public PDF(string fullFilePath) : this(File.OpenRead(fullFilePath)) { }
        
        public PDF(Stream stream) 
        {
            ObjectReference root;
            XREFTable table;
            PDFReader.readPDF(this, stream, out table, out version, out root);

            this.store = new ObjectStore(this, table, stream);
            this.catalog = root.get<Catalog>();;
        }

        public PDF()
        {
            this.store = new ObjectStore(this);
            this.version = 1.4f;
            this.catalog = new Catalog(this);
        }
        
        public T get<T>(int objectNumber, int generation)
        {
            return store.get<T>(objectNumber, generation);
        }

        public T get<T>(ObjectReference indirectReference)
        {
            return store.get<T>(indirectReference);
        }

        public void save(string fullFilePath)
        {
            PDFWriter writer = new PDFWriter(File.Create(fullFilePath), false);
            writer.writeNewPDF(this);
        }

        public Page addPage(Page page)
        {
            return insertPage(page, catalog.getNumPages() + 1);
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

        public int getNumPages()
        {
            return catalog.getNumPages();
        }

        public IEnumerator<Page> GetEnumerator() => catalog.GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => catalog.GetEnumerator();
    }
}
