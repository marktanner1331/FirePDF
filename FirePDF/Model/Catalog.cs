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
    public class Catalog : IEnumerable<Page>
    {
        private PDF pdf;
        private Dictionary<Name, object> underlyingDict;
        internal PageTreeNode pagesRoot;

        public bool isDirty => pagesRoot.isDirty;

        public Catalog(PDF pdf, Dictionary<Name, object> underlyingDict)
        {
            this.pdf = pdf;
            this.underlyingDict = underlyingDict;

            Dictionary<Name, object> pagesDict = PDFReader.readIndirectDictionary(pdf, (ObjectReference)underlyingDict["Pages"]);
            pagesRoot = new PageTreeNode(pdf, pagesDict);
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
            underlyingDict["Pages"] = pagesRoot.serialize(pdfWriter);
            return pdfWriter.writeIndirectObjectUsingNextFreeNumber(underlyingDict);
        }
    }
}
