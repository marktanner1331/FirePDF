using FirePDF.Model;
using FirePDF.Reading;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FirePDF.StreamHelpers
{
    class FlateStreamReader
    {
        /// <summary>
        /// decompresses a stream from the pdf stream at the current position and returns it
        /// </summary>
        public static MemoryStream decompressStream(PDF pdf, Stream pdfStream, PDFDictionary streamDictionary)
        {
            //http://george.chiramattel.com/blog/2007/09/deflatestream-block-length-does-not-match.html
            pdfStream.Position += 2;

            int length = streamDictionary.get<int>("Length") - 2;

            byte[] buffer = new byte[length];
            pdfStream.Read(buffer, 0, length);

            PDFReader.skipOverWhiteSpace(pdfStream);
            string chunk = ASCIIReader.readASCIIString(pdfStream, 9);
            if (chunk != "endstream")
            {
                throw new Exception();
            }

            using (DeflateStream decompressionStream = new DeflateStream(new MemoryStream(buffer), CompressionMode.Decompress))
            {
                MemoryStream decompressed = new MemoryStream();
                decompressionStream.CopyTo(decompressed);

                if (streamDictionary.ContainsKey("DecodeParms") == false)
                {
                    decompressed.Seek(0, SeekOrigin.Begin);
                    return decompressed;
                }

                PDFDictionary decodeParms = streamDictionary.get<PDFDictionary>("DecodeParms");

                int predictor = 1;
                if (decodeParms.ContainsKey("Predictor"))
                {
                    predictor = decodeParms.get<int>("Predictor");
                }

                if (predictor == 1)
                {
                    decompressed.Seek(0, SeekOrigin.Begin);
                    return decompressed;
                }

                int columns = decodeParms.get<int>("Columns");

                int colors = 1;
                if (decodeParms.ContainsKey("Colors"))
                {
                    colors = decodeParms.get<int>("Colors");
                }

                int bitsPerComponent = 8;
                if (streamDictionary.ContainsKey("BitsPerComponent"))
                {
                    bitsPerComponent = streamDictionary.get<int>("BitsPerComponent");
                }

                int bytesPerPixel = colors * bitsPerComponent / 8;

                byte[] predictedBytes = decompressed.ToArray();
                byte[] plainBytes = PNGPredictor.decompress(predictedBytes, columns, bytesPerPixel);

                decompressed.Dispose();
                return new MemoryStream(plainBytes);
            }
        }
    }
}
