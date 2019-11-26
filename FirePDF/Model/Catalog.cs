using FirePDF.Reading;
using FirePDF.Writing;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FirePDF.Model
{
    public class Catalog : IHaveUnderlyingDict, IEnumerable<Page>
    {
        internal PageTreeNode pagesRoot;
        
        public Catalog(PDFDictionary underlyingDict) : base(underlyingDict)
        {
            pagesRoot = underlyingDict.get<PageTreeNode>("Pages");
        }

        public Catalog(PDF pdf) : base(new PDFDictionary(pdf))
        {
            pagesRoot = new PageTreeNode(pdf);

            underlyingDict.set("Type", (Name)"Catalog");
            //we really want a new object here, so we aren't going to check the cache to see if it already exists
            underlyingDict.set("Pages", pdf.store.add(pagesRoot, false));
        }
        
        public int getNumPages()
        {
            return pagesRoot.getNumPages();
        }

        public Page getPage(int oneBasedPageNumber)
        {
            return pagesRoot.getPage(oneBasedPageNumber);
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

            public Page Current
            {
                get
                {
                    return current == 0 ? null : catalog.getPage(current);
                }
            }

            object IEnumerator.Current => Current;

            public void Dispose()
            {
                catalog = null;
            }

            public bool MoveNext()
            {
                current++;
                return current <= catalog.getNumPages();
            }

            public void Reset()
            {
                current = 0;
            }
        }

        internal ObjectReference serialize(PDFWriter pdfWriter)
        {
            throw new Exception("not handling saving yet");
            return null;
            //underlyingDict["Pages"] = pagesRoot.serialize(pdfWriter);
            //return pdfWriter.writeIndirectObjectUsingNextFreeNumber(underlyingDict);
        }

        internal void insertPage(Page newPage, ObjectReference objRef, int oneBasedPageNumber)
        {
            pagesRoot.insertPage(newPage, objRef, oneBasedPageNumber);
        }
    }
}
