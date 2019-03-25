using FirePDF.Reading;
using FirePDF.Writing;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace FirePDF.Model
{
    public class XREFTable
    {
        public struct XREFRecord
        {
            public int objectNumber;
            public int generation;
            public bool isCompressed;

            /// <summary>
            /// if isCompressed is false, this is the offset of the object
            /// </summary>
            public long offset;

            /// <summary>
            /// if isCompressed, this is the number of the object whose stream stores this object
            /// </summary>
            public int compressedObjectNumber;
        }

        private Dictionary<long, XREFRecord> usedRecords;

        /// <summary>
        /// a set of free records, stored as hashes
        /// </summary>
        private HashSet<long> freeRecords;

        public XREFTable()
        {
            usedRecords = new Dictionary<long, XREFRecord>();
            freeRecords = new HashSet<long>();
        }

        /// <summary>
        /// manually add an xref record to the table. if it already exists then it will be overwritten
        /// </summary>
        public void addRecord(XREFRecord record)
        {
            long hash = ((record.objectNumber & 0xffffffff) << 16) + (record.generation & 0xffff);
            usedRecords[hash] = record;

            //just in case its marked as free
            freeRecords.Remove(hash);
        }
        
        public int getNextFreeRecordNumber()
        {
            //TODO should probably do something with the free records here

            //also might not need the select as the keys should still be well-ordered
            //as the object numbers form the most significant bits

            return usedRecords.Values.Select(x => x.objectNumber).Max() + 1;
        }

        public void clear()
        {
            usedRecords.Clear();
            freeRecords.Clear();
        }

        /// <summary>
        /// marks an object as free. if it already exists as a used record then it is skipped
        /// </summary>
        public void addFreeRecord(int objectNumber, int generation)
        {
            long hash = ((objectNumber & 0xffffffff) << 16) + (generation & 0xffff);

            if (usedRecords.ContainsKey(hash))
            {
                return;
            }

            freeRecords.Add(hash);
        }

        internal void serialize(PDFWriter pdfWriter)
        {
            pdfWriter.writeASCII("xref");
            pdfWriter.writeNewLine();

            if(freeRecords.Any())
            {
                throw new NotImplementedException();
            }

            List<XREFRecord> records = usedRecords.Select(x => x.Value).OrderBy(x => x.objectNumber).ToList();

            List<XREFRecord> subRecords = new List<XREFRecord>();
            subRecords.Add(records.First());

            foreach(XREFRecord record in records.Skip(1))
            {
                if(subRecords.Last().objectNumber + 1 != record.objectNumber)
                {
                    serializeSubSection(pdfWriter, subRecords);
                    subRecords.Clear();
                }

                subRecords.Add(record);
            }

            serializeSubSection(pdfWriter, subRecords);
        }

        private void serializeSubSection(PDFWriter pdfWriter, List<XREFRecord> records)
        {
            pdfWriter.writeASCII(records.First().objectNumber + " " + records.Count);
            pdfWriter.writeNewLine();

            foreach(XREFRecord record in records)
            {
                pdfWriter.writeASCII(record.offset.ToString("0000000000") + " " + record.generation.ToString("000000") + " n\r\n");
            }
        }

        private void getRecordFromHash(long hash, out int objectNumber, out int generation)
        {
            generation = (int)(hash & 0xffff);
            hash >>= 16;

            objectNumber = (int)(hash & 0xffffffff);
        }

        public XREFRecord getXREFRecord(ObjectReference indirectReference)
        {
            return getXREFRecord(indirectReference.objectNumber, indirectReference.generation);
        }

        public XREFRecord getXREFRecord(int objectNumber, int generation)
        {
            long hash = ((objectNumber & 0xffffffff) << 16) + (generation & 0xffff);
            return usedRecords[hash];
        }

        /// <summary>
        /// merges the given table into this table
        /// if a record exists in both tables then it will not be overwritten
        /// </summary>
        public void mergeIn(XREFTable table)
        {
            foreach(var pair in table.usedRecords)
            {
                if(usedRecords.ContainsKey(pair.Key))
                {
                    continue;
                }

                usedRecords[pair.Key] = pair.Value;
            }

            foreach(long hash in table.freeRecords)
            {
                freeRecords.Add(hash);
            }

            freeRecords.RemoveWhere(x => usedRecords.ContainsKey(x));
        }

        /// <summary>
        /// parses an xref table from the stream at the current position
        /// </summary>
        public void fromStream(Stream stream)
        {
            long lastPosition = stream.Position;
            while (parseXREFSection(stream))
            {
                lastPosition = stream.Position;
            }

            stream.Position = lastPosition;
        }

        private bool parseXREFSection(Stream stream)
        {
            string firstLine = ASCIIReader.readLine(stream);
            if (firstLine != "xref")
            {
                return false;
            }
            
            long lastPosition = stream.Position;
            while (parseXREFSubSection(stream))
            {
                lastPosition = stream.Position;
            }

            stream.Position = lastPosition;

            return true;
        }

        private bool parseXREFSubSection(Stream stream)
        {
            string header = ASCIIReader.readLine(stream);
            Match headerMatch = Regex.Match(header, @"^(\d+) (\d+)$");

            if (headerMatch.Success == false)
            {
                return false;
            }

            int firstObjectNumber = int.Parse(headerMatch.Groups[1].Value);
            int length = int.Parse(headerMatch.Groups[2].Value);

            for (int i = 0; i < length; i++)
            {
                int objectNumber = firstObjectNumber + i;

                string record = ASCIIReader.readLine(stream);

                long offset = long.Parse(record.Substring(0, 10));
                int generation = int.Parse(record.Substring(11, 6));
                char type = record[17];

                long hash = ((objectNumber & 0xffffffff) << 16) + (generation & 0xffff);

                if (type == 'n')
                {
                    usedRecords[hash] = new XREFRecord
                    {
                        objectNumber = objectNumber,
                        generation = generation,
                        offset = offset
                    };
                }
                else
                {
                    freeRecords.Add(hash);
                }
            }

            return true;
        }
    }
}
