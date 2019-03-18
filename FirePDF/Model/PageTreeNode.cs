using FirePDF.Reading;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FirePDF.Model
{
    public class PageTreeNode
    {
        private PDF pdf;
        private List<object> pages;

        public PageTreeNode(PDF pdf, Dictionary<string, object> underlyingDict)
        {
            this.pdf = pdf;
            pages = new List<object>();

            foreach(ObjectReference objectReference in (List<object>)underlyingDict["Kids"])
            {
                Dictionary<string, object> kidsDict = PDFReaderLayer1.readIndirectDictionary(pdf, objectReference);
                switch(kidsDict["Type"])
                {
                    case "Page":
                        Page page = new Page(pdf, kidsDict);
                        pages.Add(page);
                        break;
                    case "Pages":
                        PageTreeNode node = new PageTreeNode(pdf, kidsDict);
                        pages.Add(node);
                        break;
                    default:
                        throw new Exception();
                }
            }
        }

        public Page getPage(int oneBasedPageNumber)
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

        public int getNumPages()
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
