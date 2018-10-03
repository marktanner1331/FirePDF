using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace FirePDF
{
    public class XREFTable
    {
        private Dictionary<long, long> usedRecords;

        public XREFTable()
        {
            usedRecords = new Dictionary<long, long>();
        }

        public long getOffsetForRecord(int objectNumber, int generation)
        {
            return usedRecords[(((long)objectNumber) << 32) + generation];
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
            string firstLine = FileReader.readLine(stream);
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
            string header = FileReader.readLine(stream);
            Match headerMatch = Regex.Match(header, @"^(\d+) (\d+)$");

            if (headerMatch.Success == false)
            {
                return false;
            }

            int firstObjectNumber = int.Parse(headerMatch.Groups[1].Value);
            int length = int.Parse(headerMatch.Groups[2].Value);

            for (int i = 0; i < length; i++)
            {
                string record = FileReader.readLine(stream);

                long offset = long.Parse(record.Substring(0, 10));
                int generation = int.Parse(record.Substring(11, 6));
                char type = record[17];

                if (type == 'n')
                {
                    long objectNumber = firstObjectNumber + i;
                    long hash = objectNumber << 32;
                    hash += generation;

                    usedRecords[hash] = offset;
                }
            }

            return true;
        }
    }
}
