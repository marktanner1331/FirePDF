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
            Dictionary<string, object> dict = PDFObjectReader.readIndirectDictionary(pdf, objectReference);
            fromDictionary(dict);
        }

        public void fromDictionary(Dictionary<string, object> dict)
        {
            foreach(ObjectReference objectReference in (List<object>)dict["Kids"])
            {
                Dictionary<string, object> KidsDict = PDFObjectReader.readIndirectDictionary(pdf, objectReference);
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

        internal Page getPage(int oneBasedPageNumber)
        {
            int pageCounter = 1;
            foreach (object node in pages)
            {
                if (node is PageTreeNode)
                {
                    int numPages = ((PageTreeNode)node).getNumPages();
                    if(pageCounter + numPages > oneBasedPageNumber)
                    {
                        return ((PageTreeNode)node).getPage(oneBasedPageNumber - pageCounter + 1);
                    }
                    else
                    {
                        pageCounter += numPages;
                    }
                }
                else
                {
                    if(pageCounter == oneBasedPageNumber)
                    {
                        return (Page)node;
                    }
                    else
                    {
                        pageCounter++;
                    }
                }
            }

            throw new Exception("Page not found in catalog");
        }

        internal int getNumPages()
        {
            int numPages = 0;
            foreach(object node in pages)
            {
                if(node is PageTreeNode)
                {
                    numPages += ((PageTreeNode)node).getNumPages();
                }
                else
                {
                    numPages++;
                }
            }

            return numPages;
        }
    }
}
