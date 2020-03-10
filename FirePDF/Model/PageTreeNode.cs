using System;

namespace FirePDF.Model
{
    public class PageTreeNode : HaveUnderlyingDict
    {
        public PageTreeNode(Pdf pdf) : base(new PdfDictionary(pdf))
        {
            UnderlyingDict.Set("Type", (Name)"Pages");
            UnderlyingDict.Set("Kids", new PdfList(pdf));
            UnderlyingDict.Set("Count", 0);
        }

        public PageTreeNode(PdfDictionary underlyingDict) : base(underlyingDict)
        {
            
        }

        public Page GetPage(int oneBasedPageNumber)
        {
            int pageCounter = 1;
            foreach (object node in UnderlyingDict.Get<PdfList>("Kids").Cast<object>())
            {
                switch (node)
                {
                    case PageTreeNode treeNode:
                    {
                        int numPages = treeNode.GetNumPages();
                        if(pageCounter + numPages > oneBasedPageNumber)
                        {
                            return treeNode.GetPage(oneBasedPageNumber - pageCounter + 1);
                        }
                        else
                        {
                            pageCounter += numPages;
                        }

                        break;
                    }
                    case Page page:
                        if (pageCounter == oneBasedPageNumber)
                        {
                            return (Page)node;
                        }
                        else
                        {
                            pageCounter++;
                        }

                        break;
                    default:
                        throw new Exception("error reading page tree");
                }
            }

            throw new Exception("Page not found in catalog");
        }
        
        public void InsertPage(Page newPage, ObjectReference objRef, int oneBasedPageNumber)
        {
            UnderlyingDict.Set("Count", UnderlyingDict.Get<int>("Count") + 1);

            int pageCounter = 1;
            int i = 0;
            foreach (object node in UnderlyingDict.Get<PdfList>("Kids").Cast<object>())
            {
                if (node is PageTreeNode)
                {
                    int numPages = ((PageTreeNode)node).GetNumPages();
                    if (pageCounter + numPages > oneBasedPageNumber)
                    {
                        ((PageTreeNode)node).InsertPage(newPage, objRef, oneBasedPageNumber - pageCounter + 1);
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
            
            UnderlyingDict.Get<PdfList>("Kids").Insert(i, objRef);

            ObjectReference me = Pdf.store.ReverseGet(this);
            if(me == null)
            {
                throw new Exception();
            }

            newPage.UnderlyingDict.Set("Parent", me);
        }

        /// <summary>
        /// returns the number of pages in the Pdf
        /// </summary>
        public int GetNumPages() => UnderlyingDict.Get<int>("Count");
    }
}
