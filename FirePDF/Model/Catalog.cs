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

        public Catalog(PDF pdf)
        {
            this.pdf = pdf;
            pagesRoot = new PageTreeNode(pdf);
        }

        public void fromStream(ObjectReference objectReference)
        {
            Dictionary<string, object> dict = (Dictionary<string, object>)PDFReaderLayer1.readIndirectObject(pdf, objectReference);
            
            pagesRoot.fromStream((ObjectReference)dict["Pages"]);
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
