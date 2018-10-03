using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FirePDF
{
    public class Catalog
    {
        private PDF pdf;
        private PageTreeNode pagesRoot;

        public Catalog(PDF pdf)
        {
            this.pdf = pdf;
        }

        public void fromStream(ObjectReference objectReference)
        {
            Dictionary<string, object> dict = (Dictionary<string, object>)PDFReaderLevel1.readIndirectObject(pdf, objectReference);

            pagesRoot = new PageTreeNode(pdf);
            pagesRoot.fromStream((ObjectReference)dict["Pages"]);
        }
    }
}
