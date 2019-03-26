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
        public Stream stream;
        public XREFTable readableTable;
        internal Catalog catalog;

        internal long? offsetOfLastXRefTable;

        public float version { get; private set; }

        public PDF(string fullFilePath) : this(File.OpenRead(fullFilePath)) { }

        public PDF(Stream stream)
        {
            this.readableTable = new XREFTable();
            this.stream = stream;
            parse();
        }

        public PDF()
        {
            readableTable = new XREFTable();
            throw new NotImplementedException();
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

        private void parse()
        {
            stream.Position = 0;
            version = PDFReader.readVersion(stream);

            Queue<long> xrefOffsets = new Queue<long>();
            HashSet<long> readOffsets = new HashSet<long>();

            stream.Position = Math.Max(0, stream.Length - 1024);
            string chunk = ASCIIReader.readASCIIString(stream, 1024);
            long lastOffset = PDFReader.readLastStartXREF(chunk);
            xrefOffsets.Enqueue(lastOffset);

            Queue<long> xrefStreams = new Queue<long>();
            ObjectReference root = null;

            while (xrefOffsets.Any())
            {
                stream.Position = xrefOffsets.Dequeue();

                Trailer trailer;
                if (PDFReader.isObjectHeader(stream))
                {
                    offsetOfLastXRefTable = stream.Position;

                    XREFStream xrefStream = new XREFStream();
                    xrefStream.fromStream(this);

                    readableTable.mergeIn(xrefStream.table);

                    trailer = xrefStream.trailer;
                }
                else
                {
                    offsetOfLastXRefTable = stream.Position;
                    XREFTable table = new XREFTable();
                    table.fromStream(stream);

                    readableTable.mergeIn(table);
                    
                    trailer = PDFReader.readTrailer(stream);
                }
                
                if (root == null)
                {
                    root = trailer.root;
                }

                if (trailer.prev.HasValue && readOffsets.Contains(trailer.prev.Value) == false)
                {
                    xrefOffsets.Enqueue(trailer.prev.Value);
                }
            }

            Dictionary<Name, object> rootDict = PDFReader.readIndirectDictionary(this, root);
            catalog = new Catalog(this, rootDict);
        }
    }
}
