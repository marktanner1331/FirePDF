using FirePDF.Reading;
using FirePDF.Writing;
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
        internal Dictionary<Name, object> underlyingDict;
        internal bool isDirty;

        public PageTreeNode(PDF pdf, Dictionary<Name, object> underlyingDict)
        {
            this.pdf = pdf;
            this.pages = new List<object>();
            this.underlyingDict = underlyingDict;

            foreach(ObjectReference objectReference in (List<object>)underlyingDict["Kids"])
            {
                Dictionary<Name, object> kidsDict = PDFReader.readIndirectDictionary(pdf, objectReference);
                switch((Name)kidsDict["Type"])
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

        internal void updatePageReference(int oneBasedPageNumber, ObjectReference objectRef)
        {
            isDirty = true;

            int pageCounter = 1;
            int i = 0;
            foreach (object node in pages)
            {
                if (node is PageTreeNode)
                {
                    int numPages = ((PageTreeNode)node).getNumPages();
                    if (pageCounter + numPages > oneBasedPageNumber)
                    {
                        throw new NotImplementedException();
                    }
                    else
                    {
                        pageCounter += numPages;
                    }
                }
                else
                {
                    if (pageCounter == oneBasedPageNumber)
                    {
                        ((List<object>)underlyingDict["Kids"])[i] = objectRef;
                        return;
                    }
                    else
                    {
                        pageCounter++;
                    }
                }

                i++;
            }

            throw new Exception("Page not found in catalog");
        }

        internal ObjectReference serialize(PDFWriter pdfWriter)
        {
            List<object> kidsArray = (List<object>)underlyingDict["Kids"];
            for (int i = 0; i < kidsArray.Count; i++)
            {
                if(pages[i] is Page)
                {
                    //this page won't be dirty anymore, although, should we serialize the page at this point?
                    //rather than doing it in the PDFWriter?
                }
                else //pages[i] is PageTreeRoot
                {
                    kidsArray[i] = pdfWriter.writeIndirectObjectUsingNextFreeNumber(pages[i]);
                }
            }

            return pdfWriter.writeIndirectObjectUsingNextFreeNumber(underlyingDict);
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
