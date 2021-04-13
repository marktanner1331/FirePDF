using System;
using System.Linq;
using System.Collections.Generic;
using System.IO;
using FirePDF.Reading;

namespace FirePDF.Model
{
    //Pdf 7.5.8
    public class XrefStream : PdfStream
    {
        public Trailer Trailer { get; private set; }
        public XrefTable Table { get; private set; }

        public XrefStream(Stream stream, PdfDictionary dict, long startOfStream) : base(stream, dict, startOfStream)
        {
            Trailer = new Trailer(dict);

            //preserving the stream's position
            //just in case the calling code isn't expecting it to be updated
            long temp = stream.Position;

            stream.Position = startOfStream;

            using (Stream inner = base.GetDecompressedStream())
            {
                int size = dict.Get<int>("Size");

                //An array containing a pair of integers for each subsection in this section.
                //Default value: [0 Size].
                List<int> index;
                if (dict.ContainsKey("Index"))
                {
                    index = dict.Get<PdfList>("Index").Cast<int>();
                }
                else
                {
                    index = new List<int> { 0, size };
                }

                List<int> w = dict.Get<PdfList>("W").Cast<int>().ToList();

                List<Tuple<int, int>> sections = new List<Tuple<int, int>>();
                for (int i = 0; i < index.Count; i += 2)
                {
                    sections.Add(new Tuple<int, int>(index[i], index[i + 1]));
                }

                int entryLength = w.Sum();

                Table = new XrefTable();
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
                                Table.AddFreeRecord(objectNumber, field3);
                                break;
                            case 1:
                                Table.AddRecord(new XrefTable.XrefRecord
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
                                Table.AddRecord(new XrefTable.XrefRecord
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

            stream.Position = temp;
        }
    }
}