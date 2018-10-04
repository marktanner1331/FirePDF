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
        public XREFTable readTable;
        private Catalog catalog;

        public PDF(string fullFilePath) : this(File.OpenRead(fullFilePath))
        {

        }

        public PDF(Stream stream) : this()
        {
            this.stream = stream;
            
            parse();
        }

        public PDF()
        {
            readTable = new XREFTable();
            catalog = new Catalog(this);
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
            string chunk = FileReader.readASCIIString(stream, 1024);
            long lastOffset = PDFReaderLevel1.readLastStartXREF(chunk);
            xrefOffsets.Enqueue(lastOffset);
            
            Queue<long> xrefStreams = new Queue<long>();
            ObjectReference root = null;
            
            while (xrefOffsets.Any())
            {
                stream.Position = xrefOffsets.Dequeue();

                XREFTable table = new XREFTable();
                table.fromStream(stream);

                readTable.mergeIn(table);

                Trailer trailer = new Trailer();
                trailer.fromStream(stream);

                if(root == null)
                {
                    root = trailer.root;
                }

                if (trailer.prev.HasValue && readOffsets.Contains(trailer.prev.Value) == false)
                {
                    xrefOffsets.Enqueue(trailer.prev.Value);
                }
            }

            catalog.fromStream(root);
        }
    }
}
