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
        private Stream stream;

        public PDF(string fullFilePath) : this(File.OpenRead(fullFilePath))
        {

        }

        public PDF(Stream stream)
        {
            this.stream = stream;
            parse();
        }

        private void parse()
        {
            parseXREFTables();
        }

        private void parseXREFTables()
        {
            stream.Position = Math.Max(0, stream.Length - 1024);
            string chunk = FileReader.readASCIIString(stream, 1024);
            long xrefOffset = PDFReaderLevel1.readLastStartXREF(chunk);

            stream.Position = xrefOffset;
            XREFTable table = new XREFTable();
            table.fromStream(stream);
        }
    }
}
