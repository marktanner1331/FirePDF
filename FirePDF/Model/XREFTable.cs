using FirePDF.Reading;
using FirePDF.Writing;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;

namespace FirePDF.Model
{
    public class XrefTable
    {
        public struct XrefRecord
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

        private readonly Dictionary<long, XrefRecord> usedRecords;

        /// <summary>
        /// a set of free records, stored as hashes
        /// </summary>
        private readonly HashSet<long> freeRecords;

        public XrefTable()
        {
            usedRecords = new Dictionary<long, XrefRecord>();
            freeRecords = new HashSet<long>();
        }

        /// <summary>
        /// manually add an xref record to the table. if it already exists then it will be overwritten
        /// </summary>
        public void AddRecord(XrefRecord record)
        {
            long hash = ((record.objectNumber & 0xffffffff) << 16) + (record.generation & 0xffff);
            usedRecords[hash] = record;

            //just in case its marked as free
            freeRecords.Remove(hash);
        }
        
        public int GetNextFreeRecordNumber()
        {
            //TODO should probably do something with the free records here

            //also might not need the select as the keys should still be well-ordered
            //as the object numbers form the most significant bits

            if(usedRecords.Count == 0)
            {
                return 1;
            }
            else
            {
                return usedRecords.Values.Select(x => x.objectNumber).Max() + 1;
            }
        }

        public void Clear()
        {
            usedRecords.Clear();
            freeRecords.Clear();
        }

        /// <summary>
        /// marks an object as free. if it already exists as a used record then it is skipped
        /// </summary>
        public void AddFreeRecord(int objectNumber, int generation)
        {
            long hash = ((objectNumber & 0xffffffff) << 16) + (generation & 0xffff);

            if (usedRecords.ContainsKey(hash))
            {
                return;
            }

            freeRecords.Add(hash);
        }

        internal void Serialize(PdfWriter pdfWriter)
        {
            pdfWriter.WriteAscii("xref");
            pdfWriter.WriteNewLine();

            if(freeRecords.Any())
            {
                throw new NotImplementedException();
            }

            List<XrefRecord> records = usedRecords.Select(x => x.Value).OrderBy(x => x.objectNumber).ToList();

            List<XrefRecord> subRecords = new List<XrefRecord>();
            subRecords.Add(records.First());

            foreach(XrefRecord record in records.Skip(1))
            {
                if(subRecords.Last().objectNumber + 1 != record.objectNumber)
                {
                    SerializeSubSection(pdfWriter, subRecords);
                    subRecords.Clear();
                }

                subRecords.Add(record);
            }

            SerializeSubSection(pdfWriter, subRecords);
        }

        public bool HasXrefRecord(ObjectReference indirectReference)
        {
            return usedRecords.ContainsKey(indirectReference.GetHashCode());
        }

        private static void SerializeSubSection(PdfWriter pdfWriter, List<XrefRecord> records)
        {
            pdfWriter.WriteAscii(records.First().objectNumber + " " + records.Count);
            pdfWriter.WriteNewLine();

            foreach(XrefRecord record in records)
            {
                pdfWriter.WriteAscii(record.offset.ToString("0000000000") + " " + record.generation.ToString("00000") + " n\r\n");
            }
        }

        public IEnumerable<XrefRecord> GetAllXrefRecords()
        {
            return usedRecords.Values;
        }

        private void GetRecordFromHash(long hash, out int objectNumber, out int generation)
        {
            generation = (int)(hash & 0xffff);
            hash >>= 16;

            objectNumber = (int)(hash & 0xffffffff);
        }

        public XrefRecord? GetXrefRecord(ObjectReference indirectReference)
        {
            return GetXrefRecord(indirectReference.objectNumber, indirectReference.generation);
        }

        public XrefRecord? GetXrefRecord(int objectNumber, int generation)
        {
            long hash = ((objectNumber & 0xffffffff) << 16) + (generation & 0xffff);
            if(usedRecords.ContainsKey(hash))
            {
                return usedRecords[hash];
            }
            else
            {
                return null;
            }
        }

        /// <summary>
        /// merges the given table into this table
        /// if a record exists in both tables then it will not be overwritten
        /// </summary>
        public void MergeIn(XrefTable table)
        {
            foreach(KeyValuePair<long, XrefRecord> pair in table.usedRecords)
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
        public void FromStream(Stream stream)
        {
            long lastPosition = stream.Position;
            while (ParseXrefSection(stream))
            {
                lastPosition = stream.Position;
            }

            stream.Position = lastPosition;
        }

        private bool ParseXrefSection(Stream stream)
        {
            string firstLine = AsciiReader.ReadLine(stream);
            if (firstLine != "xref")
            {
                return false;
            }
            
            long lastPosition = stream.Position;
            while (ParseXrefSubSection(stream))
            {
                lastPosition = stream.Position;
            }

            stream.Position = lastPosition;

            return true;
        }

        private bool ParseXrefSubSection(Stream stream)
        {
            string header = AsciiReader.ReadLine(stream);
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

                string record = AsciiReader.ReadLine(stream);

                long offset = long.Parse(record.Substring(0, 10));
                int generation = int.Parse(record.Substring(11, 6));
                char type = record[17];

                long hash = ((objectNumber & 0xffffffff) << 16) + (generation & 0xffff);

                if (type == 'n')
                {
                    usedRecords[hash] = new XrefRecord
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
