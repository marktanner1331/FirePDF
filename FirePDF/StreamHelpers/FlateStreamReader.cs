using FirePDF.Model;
using FirePDF.Reading;
using System;
using System.IO;
using System.IO.Compression;

namespace FirePDF.StreamHelpers
{
    public class FlateStreamReader
    {
        /// <summary>
        /// decompresses a stream from the Pdf stream at the current position and returns it
        /// </summary>
        public static MemoryStream DecompressStream(Pdf pdf, Stream pdfStream, PdfDictionary streamDictionary)
        {
            //http://george.chiramattel.com/blog/2007/09/deflatestream-block-length-does-not-match.html
            long startOfStream = pdfStream.Position + 2;

            //hint: the .get() here might update the position of the stream (if its an indirect object)
            int length = streamDictionary.Get<int>("Length") - 2;

            pdfStream.Position = startOfStream;
            byte[] buffer = new byte[length];
            pdfStream.Read(buffer, 0, length);

            PdfReader.SkipOverWhiteSpace(pdfStream);
            string chunk = AsciiReader.ReadAsciiString(pdfStream, 9);
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

                PdfDictionary decodeParms = streamDictionary.Get<PdfDictionary>("DecodeParms");

                int predictor = 1;
                if (decodeParms.ContainsKey("Predictor"))
                {
                    predictor = decodeParms.Get<int>("Predictor");
                }

                if (predictor == 1)
                {
                    decompressed.Seek(0, SeekOrigin.Begin);
                    return decompressed;
                }

                int columns = decodeParms.Get<int>("Columns");

                int colors = 1;
                if (decodeParms.ContainsKey("Colors"))
                {
                    colors = decodeParms.Get<int>("Colors");
                }

                int bitsPerComponent = 8;
                if (streamDictionary.ContainsKey("BitsPerComponent"))
                {
                    bitsPerComponent = streamDictionary.Get<int>("BitsPerComponent");
                }

                int bytesPerPixel = colors * bitsPerComponent / 8;

                byte[] predictedBytes = decompressed.ToArray();
                byte[] plainBytes = PngPredictor.Decompress(predictedBytes, columns, bytesPerPixel);

                decompressed.Dispose();
                return new MemoryStream(plainBytes);
            }
        }
    }
}
