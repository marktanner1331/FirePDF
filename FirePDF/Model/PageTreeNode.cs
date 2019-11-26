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
    public class PageTreeNode : IHaveUnderlyingDict
    {
        public PageTreeNode(PDF pdf) : base(new PDFDictionary(pdf))
        {
            underlyingDict.set("Type", (Name)"Pages");
            underlyingDict.set("Kids", new PDFList(pdf));
        }

        public PageTreeNode(PDFDictionary underlyingDict) : base(underlyingDict)
        {
            
        }

        public Page getPage(int oneBasedPageNumber)
        {
            int pageCounter = 1;
            foreach (object node in underlyingDict.get<PDFList>("Kids").cast<object>())
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
                else if(node is Page)
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
                else
                {
                    throw new Exception("error reading page tree");
                }
            }

            throw new Exception("Page not found in catalog");
        }
        
        public void insertPage(Page newPage, ObjectReference objRef, int oneBasedPageNumber)
        {
            underlyingDict.set("Count", underlyingDict.get<int>("Count") + 1);

            int pageCounter = 1;
            int i = 0;
            foreach (object node in underlyingDict.get<PDFList>("Kids").cast<object>())
            {
                if (node is PageTreeNode)
                {
                    int numPages = ((PageTreeNode)node).getNumPages();
                    if (pageCounter + numPages > oneBasedPageNumber)
                    {
                        ((PageTreeNode)node).insertPage(newPage, objRef, oneBasedPageNumber - pageCounter + 1);
                        return;
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
                        break;
                    }
                    else
                    {
                        pageCounter++;
                    }
                }

                i++;
            }
            
            underlyingDict.get<PDFList>("Kids").insert(i, objRef);

            ObjectReference me = pdf.store.reverseGet(this);
            if(me == null)
            {
                throw new Exception();
            }

            newPage.underlyingDict.set("Parent", me);
        }

        public int getNumPages()
        {
            int numPages = 0;
            foreach (object node in underlyingDict.get<PDFList>("Kids").cast<object>())
            {
                if (node is PageTreeNode)
                {
                    numPages += ((PageTreeNode)node).getNumPages();
                }
                else if (node is Page)
                {
                    numPages++;
                }
                else
                {
                    throw new Exception("error reading page tree");
                }
            }

            return numPages;
        }
    }
}
