using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;

namespace Samples
{
    public static class CSVFunctions
    {
        /// <summary>
        /// converts a collection of objects to CSV format and returns it as a string
        /// </summary>
        public static string dataToCSV<T>(IEnumerable<T> objects)
        {
            StringBuilder sb = new StringBuilder();
            StringWriter writer = new StringWriter(sb);
            dataToCSV(objects, writer);

            return sb.ToString();
        }

        /// <summary>
        /// converts a collection of objects to CSV format and writes it to a file
        /// </summary>
        public static void dataToCSV<T>(IEnumerable<T> objects, string outputPath)
        {
            StreamWriter sw = new StreamWriter(File.Create(outputPath));
            dataToCSV(objects, sw);
        }

        /// <summary>
        /// converts a collection of objects to CSV format and writes it to the given text writer
        /// </summary>
        public static void dataToCSV<T>(IEnumerable<T> objects, TextWriter textWriter)
        {
            var cols = objects.First().GetType().GetFields(BindingFlags.Public | BindingFlags.Instance).ToList();
            textWriter.WriteLine(string.Join(", ", cols.Select(x => x.Name)));

            foreach (T obj in objects)
            {
                bool isFirst = true;
                foreach (var col in cols)
                {
                    string value = (col.GetValue(obj) ?? "null").ToString();

                    if (value.Contains(","))
                    {
                        value = "\"" + value + "\"";
                    }

                    if (isFirst == false)
                    {
                        textWriter.Write(", " + value);
                    }
                    else
                    {
                        textWriter.Write(value);
                        isFirst = false;
                    }
                }

                textWriter.WriteLine();
            }

            textWriter.Flush();
            textWriter.Close();
            textWriter.Close();
        }

        /// <summary>
        /// parses a CSV file and returns it as a dictionary in which the keys are the column names and the values are arrays containing the data
        /// </summary>
        /// <param name="CSVData">the CSV data as a string</param>
        /// <param name="firstRowIsHeader">if the first row contains the names of the columns then this should be set to true</param>
        public static unsafe Dictionary<string, string[]> parseCSV(string CSVData, bool firstRowIsHeader)
        {
            Dictionary<string, List<string>> table = new Dictionary<string, List<string>>();

            fixed (char* data = CSVData)
            {
                char* start = data;
                char* end = start + CSVData.Length;

                List<string> header;
                if (firstRowIsHeader)
                {
                    header = new List<string>();
                    parseRow(ref start, end, header.Add);
                    foreach (string headerItem in header)
                    {
                        table.Add(headerItem, new List<string>());
                    }
                }
                else
                {
                    header = new List<string>();
                    List<string> firstRow = new List<string>();
                    parseRow(ref start, end, firstRow.Add);

                    for (int i = 0; i < firstRow.Count; i++)
                    {
                        string headerItem = "Row " + i.ToString();
                        header.Add(headerItem);
                        table.Add(headerItem, new List<string> { firstRow[i] });
                    }
                }

                while (start != end)
                {
                    int i = 0;
                    parseRow(ref start, end, x =>
                    {
                        table[header[i]].Add(x);
                        i++;
                    });
                }
            }

            return table.ToDictionary(x => x.Key, x => x.Value.ToArray());
        }

        private static unsafe void parseRow(ref char* data, char* end, Action<string> foundCell)
        {
            bool insideQuote = false;
            StringBuilder sb = new StringBuilder();

            while (data != end)
            {
                switch (*data)
                {
                    case '\r':
                    case '\n':
                        foundCell(sb.ToString());

                        while (*data == '\r' || *data == '\n')
                            data++;

                        return;
                    case '"':
                        insideQuote = !insideQuote;
                        break;
                    case ',':
                        if (insideQuote)
                        {
                            goto default;
                        }
                        else
                        {
                            foundCell(sb.ToString());
                            sb.Clear();
                        }
                        break;
                    default:
                        sb.Append(*data);
                        break;
                }

                data++;
            }
        }
    }
}
