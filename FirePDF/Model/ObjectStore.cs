using FirePDF.Reading;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FirePDF.Model
{
    /// <summary>
    /// this class manages reading, caching, and writing objects to the various pdf streams
    /// </summary>
    public class ObjectStore
    {
        //objects can either be stored in the existing table or the new table
        //if we are reading an existing pdf then everything will be in the existing table
        //but if we are adding new bits to it, or constructing a pdf from scratch then it will be in the newTable
        //object numbers will be unique, i.e. you wont find the same number in both tables

        //TODO: add a cache, should be nice and easy, we can even cache things like images, as its only the head that gets cached (i.e. the dict and offset)

        private PDF pdf;
        
        private XREFTable existingTable;
        private Stream existingStream;

        private XREFTable newTable;
        private Stream newStream;

        public ObjectStore(PDF pdf, XREFTable existingTable, Stream existingStream)
        {
            this.pdf = pdf;
            this.existingTable = existingTable;
            this.existingStream = existingStream;
            this.newTable = new XREFTable();
            this.newStream = new MemoryStream();
        }

        //it is ok for the existingStream to be null
        //because if nothing is in the existingTable then we shouldn't even try to look it up in the stream
        public ObjectStore(PDF pdf) : this(pdf, new XREFTable(), null) { }

        public T get<T>(int objectNumber, int generation)
        {
            XREFTable.XREFRecord? record = existingTable.getXREFRecord(objectNumber, generation);
            if (record != null)
            {
                return (T)PDFReader.readIndirectObject(pdf, existingStream, record.Value);
            }
            else
            {
                record = newTable.getXREFRecord(objectNumber, generation);
                return (T)PDFReader.readIndirectObject(pdf, newStream, record.Value);
            }
        }

        public T get<T>(ObjectReference indirectReference) => get<T>(indirectReference.objectNumber, indirectReference.generation);
    }
}
