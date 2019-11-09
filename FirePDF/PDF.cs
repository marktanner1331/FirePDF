using FirePDF.Model;
using FirePDF.Reading;
using FirePDF.Writing;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace FirePDF
{
    public class PDF
    {
        internal ObjectStore store;
        internal Catalog catalog;
        
        public readonly float version;

        public PDF(string fullFilePath) : this(File.OpenRead(fullFilePath)) { }

        /*
         * if we use a PDFWriter
         * then this means we don't have to worry about saving in the PDF
         * and PDF can be only for reading
         * but we will still want to do a PDFWriter.addPage()
         * and those pages are going to need empty constructors
         * and Page will need to a reference to PDF
         * as it will have resources
         * and will need to look up references
         * as we cant store everything in memory all the time
         * so we will need to support some kind of
         * PDF.addImage()
         * but that means a separate write stream
         * as it will need to go to disk
         * but can't be part of the read stream
         * so maybe a temporary stream
         * but then
         * if we do that, and we have multiple streams
         * and then we need to access something 
         * we need to figure out which stream it's in
         * when calling PDFReader
         * unless we have some kind of object getter
         * that takes an object reference
         * and gives back the object
         * we need to implement caching at some point anyway
         * and that is how i was going to do it
         * so this object cache
         * would hold the xref table
         * the file stream
         * and the modification stream
         * and everytime you request something
         * it figures out where it needs to come from
         * and also lets you add new things
         * and handles creating and tracking new indirect references
         * that could work nicely
         * and could even handle updates as well as fresh saves maybe
         * 
         * ok, so lets try a typical flow
         * instead of things like PDFDictionary calling PDFReader directly
         * it will call pdf.get<>() 
         * and this will call the objectStore
         * which will handle returning the cached copy
         * or reading it from the file stream and caching
         * or reading from the modification stream and caching
         * 
         * if we call new PDF()
         * it will create a store with only a modification stream
         * 
         * if we call new Page(PDF)
         * it will fail
         * 
         * maybe instead we use PDF.addPage() or PDF.insertPage()
         * and that returns a new Page
         * 
         * but what if we want to add an existing page from another pdf
         * handy if we are merging or bursting
         * 
         * then it makes sense to be able to do a new Page(PDF)
         * 
         * also, for some random reason, someone might try to re-add an existing page to the end of a pdf
         * 
         * i think definitely addPage() needs to take a page object
         * but then what do we do with new pages
         * new Page(PDF) does make a lot of sense
         * 
         * but if we never call addPage() on it then we don't want it add resources and stuff
         * although upon saving those will get cleaned up
         * and as for efficiency overhead, it's your own fault for creating a page that you don't want to add
         * really its no different from calling removePage()
         * as that's not going to clean up resources either (although it could, i don't see the point)
         * 
         * lets talk about update vs fresh
         * if the pdf doesn't have an existing file stream
         * then it's identical
         * but in the case that its an existing pdf
         * calling update would do what exactly?
         * well that would be down to the pdf writer
         * the store would need to keep incrementing object references regardless
         * and these new ones can be simply used when calling update
         * if calling fresh
         * then we probably want to clear out the references not being referenced
         * although we don't actually have to
         * we can have gaps if we really want
         * in that case we probably want to have a map from existing (including modified) to remapped
         * but that map should probably exist in the pdf writer, not the store
         * to reduce complexity
         * and keep all code specific to writing in the writer
         */
        
        public PDF(Stream stream)
        {
            ObjectReference root;
            XREFTable table;
            PDFReader.readPDF(this, stream, out table, out version, out root);

            this.store = new ObjectStore(this, table, stream);
            this.catalog = new Catalog(this, root.get<PDFDictionary>());
        }

        public PDF()
        {
            throw new NotImplementedException();
            //this.store = new ObjectStore();
            //readableTable = new XREFTable();
            //catalog = new Catalog(store);
            //stream = new MemoryStream();
           
        }

        public T get<T>(int objectNumber, int generation)
        {
            return store.get<T>(objectNumber, generation);
        }

        public T get<T>(ObjectReference indirectReference)
        {
            return store.get<T>(indirectReference);
        }

        public void save(string fullFilePath)
        {
           // PDFWriter.write(this, File.Create(fullFilePath));
        }

        public Page getPage(int oneBasedPageNumber)
        {
            return catalog.getPage(oneBasedPageNumber);
        }

        public int getNumPages()
        {
            return catalog.getNumPages();
        }
    }
}
