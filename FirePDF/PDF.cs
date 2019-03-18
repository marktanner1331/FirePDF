using FirePDF.Model;
using FirePDF.Reading;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FirePDF
{
    public class PDF
    {
        public Stream stream;
        public XREFTable readableTable;
        private Catalog catalog;

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
                    XREFStream xrefStream = new XREFStream();
                    xrefStream.fromStream(this);

                    readableTable.mergeIn(xrefStream.table);

                    trailer = xrefStream.trailer;
                }
                else
                {
                    XREFTable table = new XREFTable();
                    table.fromStream(stream);

                    readableTable.mergeIn(table);

                    trailer = new Trailer();
                    trailer.fromStream(stream);
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

            Dictionary<string, object> rootDict = PDFReader.readIndirectDictionary(this, root);
            catalog = new Catalog(this, rootDict);
        }
    }
}
