using FirePDF.Reading;
using System;
using System.IO;

namespace FirePDF.Model
{
    /// <summary>
    /// represents any Pdf object that includes a stream, including forms and images
    /// </summary>
    public class PdfStream : HaveUnderlyingDict
    {
        protected Stream stream;
        protected long startOfStream;

        public PdfStream(Stream stream, PdfDictionary underlyingDictionary, long startOfStream) : base(underlyingDictionary)
        {
            this.stream = stream;
            this.startOfStream = startOfStream;
        }

        public Stream GetCompressedStream()
        {
            Stream newStream = new ProxyStream(stream, startOfStream, UnderlyingDict.Get<int>("Length"));
            newStream.Position = 0;
            return newStream;
        }

        public Stream GetDecompressedStream()
        {
            stream.Position = startOfStream;
            return PdfReader.DecompressStream(Pdf, stream, UnderlyingDict);
        }

        public static PdfStream FromDictionary(PdfDictionary dict, Stream stream, long startOfStream)
        {
            if (dict.ContainsKey("Type") == false)
            {
                return new PdfStream(stream, dict, startOfStream);
            }

            switch (dict.Get<Name>("Type"))
            {
                case "ObjStm":
                    return new PdfObjectStream(stream, dict, startOfStream);
                case "XObject":
                    switch (dict.Get<Name>("Subtype"))
                    {
                        case "Form":
                            return new XObjectForm(stream, dict, startOfStream);
                        case "Image":
                            return new XObjectImage(stream, dict, startOfStream);
                        default:
                            throw new NotImplementedException();
                    }
                case "Font":
                    switch (dict.Get<Name>("Subtype"))
                    {
                        case "CIDFontType0C":
                        case "Type1C":
                            return new PdfStream(stream, dict, startOfStream);
                        default:
                            throw new NotImplementedException();
                    }
                case "Metadata":
                    return new PdfMetaDataStream(stream, dict, startOfStream);
                case "Pattern":
                    return new Pattern(stream, dict, startOfStream);
                case "XRef":
                    return new XrefStream(stream, dict, startOfStream);
                default:
                    throw new NotImplementedException();
            }

        }
    }
}
