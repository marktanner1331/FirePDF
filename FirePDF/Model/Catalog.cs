using FirePDF.Reading;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FirePDF.Model
{
    public class Catalog
    {
        private PDF pdf;
        private PageTreeNode pagesRoot;

        public Catalog(PDF pdf, Dictionary<string, object> underlyingDict)
        {
            this.pdf = pdf;

            Dictionary<string, object> pagesDict = PDFReaderLayer1.readIndirectDictionary(pdf, (ObjectReference)underlyingDict["Pages"]);
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
    }
}
