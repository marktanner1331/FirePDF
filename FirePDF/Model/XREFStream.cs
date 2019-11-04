using System;
using System.Linq;
using System.Collections.Generic;
using System.IO;
using FirePDF.Reading;

namespace FirePDF.Model
{
    //pdf 7.5.8
    public class XREFStream
    {
        public Trailer trailer { get; private set; }
        public XREFTable table { get; private set; }

        /// <summary>
        /// assumes the current position of the stream is the first byte of the object header
        /// </summary>
        public void fromStream(PDF pdf)
        {
            PDFReader.skipOverObjectHeader(pdf.stream);

            PDFDictionary dict = PDFReader.readDictionary(pdf, pdf.stream);
            trailer = new Trailer(dict);

            PDFReader.skipOverStreamHeader(pdf.stream);
            
            using (Stream inner = PDFReader.decompressStream(pdf, pdf.stream, dict))
            {
                int size = (int)dict["Size"];

                //An array containing a pair of integers for each subsection in this section.
                //Default value: [0 Size].
                List<int> index;
                if (dict.ContainsKey("Index"))
                {
                    index = ((List<object>)dict["Index"]).Cast<int>().ToList();
                }
                else
                {
                    index = new List<int> { 0, size };
                }

                List<int> w = ((List<object>)dict["W"]).Cast<int>().ToList();

                List<Tuple<int, int>> sections = new List<Tuple<int, int>>();
                for (int i = 0; i < index.Count; i += 2)
                {
                    sections.Add(new Tuple<int, int>(index[i], index[i + 1]));
                }

                int entryLength = w.Sum();

                table = new XREFTable();
                foreach (Tuple<int, int> section in sections)
                {
                    for (int k = 0; k < section.Item2; k++)
                    {
                        int objectNumber = k + section.Item1;

                        int field1 = 0;
                        for (int i = 0; i < w[0]; i++)
                        {
                            field1 <<= 8;
                            field1 |= inner.ReadByte();
                        }

                        int field2 = 0;
                        for (int i = 0; i < w[1]; i++)
                        {
                            field2 <<= 8;
                            field2 |= inner.ReadByte();
                        }

                        int field3 = 0;
                        for (int i = 0; i < w[2]; i++)
                        {
                            field3 <<= 8;
                            field3 |= inner.ReadByte();
                        }

                        switch (field1)
                        {
                            case 0:
                                table.addFreeRecord(objectNumber, field3);
                                break;
                            case 1:
                                table.addRecord(new XREFTable.XREFRecord
                                {
                                    objectNumber = objectNumber,
                                    offset = field2,
                                    generation = field3,
                                });
                                break;
                            case 2:
                                //field3 contains the object index
                                //however we don't actually need this
                                //as the compressed objects are stored against their object numbers, not their indices
                                table.addRecord(new XREFTable.XREFRecord
                                {
                                    objectNumber = objectNumber,
                                    compressedObjectNumber = field2,
                                    isCompressed = true
                                });
                                break;
                            default:
                                throw new NotSupportedException();
                        }
                    }
                }
            }
        }
    }
}