using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FirePDF
{
    public class PageTreeNode
    {
        private PDF pdf;
        private List<object> pages;

        public PageTreeNode(PDF pdf)
        {
            this.pdf = pdf;
            pages = new List<object>();
        }

        public void fromStream(ObjectReference objectReference)
        {
            Dictionary<string, object> dict = PDFReaderLevel1.readIndirectDictionary(pdf, objectReference);
            fromDictionary(dict);
        }

        public void fromDictionary(Dictionary<string, object> dict)
        {
            foreach(ObjectReference objectReference in (List<object>)dict["Kids"])
            {
                Dictionary<string, object> KidsDict = PDFReaderLevel1.readIndirectDictionary(pdf, objectReference);
                switch(KidsDict["Type"])
                {
                    case "Page":
                        Page page = new Page(pdf);
                        page.fromDictionary(KidsDict);
                        pages.Add(page);
                        break;
                    case "Pages":
                        PageTreeNode node = new PageTreeNode(pdf);
                        node.fromDictionary(KidsDict);
                        pages.Add(node);
                        break;
                    default:
                        throw new Exception();
                }
            }
        }
    }
}
