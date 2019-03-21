using FirePDF.Reading;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FirePDF.Model
{
    public class Catalog : IEnumerable<Page>
    {
        private PDF pdf;
        private PageTreeNode pagesRoot;

        private Dictionary<int, Page> cache;
        public bool isDirty => cache.Values.Any(x => x.isDirty);

        public Catalog(PDF pdf, Dictionary<Name, object> underlyingDict)
        {
            this.pdf = pdf;
            this.cache = new Dictionary<int, Page>();

            Dictionary<Name, object> pagesDict = PDFReader.readIndirectDictionary(pdf, (ObjectReference)underlyingDict["Pages"]);
            pagesRoot = new PageTreeNode(pdf, pagesDict);
        }

        public int getNumPages()
        {
            return pagesRoot.getNumPages();
        }

        public Page getPage(int oneBasedPageNumber)
        {
            if (cache.ContainsKey(oneBasedPageNumber))
            {
                return cache[oneBasedPageNumber];
            }
            else
            {
                cache[oneBasedPageNumber] = pagesRoot.getPage(oneBasedPageNumber);
                return cache[oneBasedPageNumber];
            }
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
    }
}
