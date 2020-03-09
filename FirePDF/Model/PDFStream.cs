using FirePDF.Reading;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FirePDF.Model
{
    /// <summary>
    /// represents any pdf object that includes a stream, including forms and images
    /// </summary>
    public class PDFStream : IHaveUnderlyingDict
    {
        protected Stream stream;
        protected long startOfStream;

        public PDFStream(Stream stream, PDFDictionary underlyingDictionary, long startOfStream) : base(underlyingDictionary)
        {
            this.stream = stream;
            this.startOfStream = startOfStream;
        }

        public Stream getCompressedStream()
        {
            Stream newStream = new ProxyStream(stream, startOfStream, underlyingDict.get<int>("Length"));
            newStream.Position = 0;
            return newStream;
        }

        public Stream getDecompressedStream()
        {
            stream.Position = startOfStream;
            return PDFReader.decompressStream(pdf, stream, underlyingDict);
        }

        public static PDFStream fromDictionary(PDFDictionary dict, Stream stream, long startOfStream)
        {
            if (dict.containsKey("Type") == false)
            {
                return new PDFStream(stream, dict, startOfStream);
            }

            switch (dict.get<Name>("Type"))
            {
                case "ObjStm":
                    return new PDFObjectStream(stream, dict, startOfStream);
                case "XObject":
                    switch (dict.get<Name>("Subtype"))
                    {
                        case "Form":
                            return new XObjectForm(stream, dict, startOfStream);
                        case "Image":
                            return new XObjectImage(stream, dict, startOfStream);
                        default:
                            throw new NotImplementedException();
                    }
                case "Font":
                    switch (dict.get<Name>("Subtype"))
                    {
                        case "CIDFontType0C":
                        case "Type1C":
                            return new PDFStream(stream, dict, startOfStream);
                        default:
                            throw new NotImplementedException();
                    }
                case "Metadata":
                    return new PDFMetaDataStream(stream, dict, startOfStream);
                default:
                    throw new NotImplementedException();
            }

        }
    }
}
