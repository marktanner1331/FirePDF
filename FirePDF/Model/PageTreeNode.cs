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
        internal PDFDictionary underlyingDict;
        internal bool isDirty => pages.Where(x => x is Page).Any(x => ((Page)x).isDirty);

        public PageTreeNode(PDF pdf, PDFDictionary underlyingDict)
        {
            this.pdf = pdf;
            this.pages = new List<object>();
            this.underlyingDict = underlyingDict;

            foreach(ObjectReference objectReference in underlyingDict.get<List<object>>("Kids"))
            {
                PDFDictionary kidsDict = PDFReader.readIndirectDictionary(pdf, objectReference);
                switch(kidsDict.get<Name>("Type"))
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
        
        internal ObjectReference serialize(PDFWriter pdfWriter)
        {
            List<object> kidsArray = underlyingDict.get<List<object>>("Kids");
            for (int i = 0; i < kidsArray.Count; i++)
            {
                if(pages[i] is Page)
                {
                    Page page = (Page)pages[i];
                    if(page.isDirty)
                    {
                        kidsArray[i] = page.serialize(pdfWriter);
                    }
                }
                else
                {
                    PageTreeNode node = (PageTreeNode)pages[i];
                    if(node.isDirty)
                    {
                        kidsArray[i] = node.serialize(pdfWriter);
                    }
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
