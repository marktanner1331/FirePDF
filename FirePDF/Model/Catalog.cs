using FirePDF.Writing;
using System;
using System.Collections;
using System.Collections.Generic;

namespace FirePDF.Model
{
    public class Catalog : HaveUnderlyingDict, IEnumerable<Page>
    {
        internal PageTreeNode pagesRoot;
        
        public Catalog(PdfDictionary underlyingDict) : base(underlyingDict)
        {
            pagesRoot = underlyingDict.Get<PageTreeNode>("Pages");
        }

        public Catalog(Pdf pdf) : base(new PdfDictionary(pdf))
        {
            pagesRoot = new PageTreeNode(pdf);

            UnderlyingDict.Set("Type", (Name)"Catalog");
            //we really want a new object here, so we aren't going to check the cache to see if it already exists
            UnderlyingDict.Set("Pages", pdf.store.Add(pagesRoot));
        }

        /// <summary>
        /// returns the number of pages in the Pdf
        /// </summary>
        public int NumPages()
        {
            return pagesRoot.GetNumPages();
        }

        public Page GetPage(int oneBasedPageNumber)
        {
            return pagesRoot.GetPage(oneBasedPageNumber);
        }

        public IEnumerator<Page> GetEnumerator()
        {
            return new CatalogEnumerator(this);
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return new CatalogEnumerator(this);
        }

        private class CatalogEnumerator : IEnumerator<Page>
        {
            private Catalog catalog;
            private int current = 0;

            public CatalogEnumerator(Catalog catalog)
            {
                this.catalog = catalog;
            }

            public Page Current => current == 0 ? null : catalog.GetPage(current);

            object IEnumerator.Current => Current;

            public void Dispose()
            {
                catalog = null;
            }

            public bool MoveNext()
            {
                current++;
                return current <= catalog.NumPages();
            }

            public void Reset()
            {
                current = 0;
            }
        }

        internal ObjectReference Serialize(PdfWriter pdfWriter)
        {
            throw new Exception("not handling saving yet");
            //underlyingDict["Pages"] = pagesRoot.serialize(pdfWriter);
            //return pdfWriter.writeIndirectObjectUsingNextFreeNumber(underlyingDict);
        }

        internal void InsertPage(Page newPage, ObjectReference objRef, int oneBasedPageNumber)
        {
            pagesRoot.InsertPage(newPage, objRef, oneBasedPageNumber);
        }
    }
}
