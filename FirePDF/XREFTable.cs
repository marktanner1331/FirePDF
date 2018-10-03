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
    class XREFTable
    {
        public XREFTable()
        {

        }

        /// <summary>
        /// parses an xref table from the stream at the current position
        /// </summary>
        public void fromStream(Stream stream)
        {
            using (StreamReader reader = new StreamReader(stream))
            {
                long lastPosition = reader.BaseStream.Position;
                while (parseXREFSection(reader))
                {
                    lastPosition = reader.BaseStream.Position;
                }

                reader.BaseStream.Position = lastPosition;
            }
        }

        private bool parseXREFSection(StreamReader reader)
        {
            string firstLine = reader.ReadLine();
            if (firstLine != "xref")
            {
                return false;
            }

            long lastPosition = reader.BaseStream.Position;
            while (parseXREFSubSection(reader))
            {
                lastPosition = reader.BaseStream.Position;
            }

            reader.BaseStream.Position = lastPosition;

            return true;
        }

        private bool parseXREFSubSection(StreamReader reader)
        {
            string header = reader.ReadLine();
            Match headerMatch = Regex.Match(header, @"^(\d+) (\d+)$");

            if(headerMatch.Success == false)
            {
                return false;
            }

            int firstObjectNumber = int.Parse(headerMatch.Groups[1].Value);
            int length = int.Parse(headerMatch.Groups[2].Value);

            for (int i = 0; i < length; i++)
            {
                string record = reader.ReadLine();

                long offset = long.Parse(record.Substring(0, 10));
                int generation = int.Parse(record.Substring(11, 6));
                char type = record[17];

                Debug.WriteLine($"{offset.ToString("0000000000")} {generation.ToString("000000")} {type}");
            }

            return true;
        }
    }
}
