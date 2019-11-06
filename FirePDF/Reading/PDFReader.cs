using FirePDF.Model;
using FirePDF.StreamHelpers;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace FirePDF.Reading
{
    public static class PDFReader
    {
        public static Stream decompressStream(PDF pdf, XREFTable.XREFRecord xrefRecord)
        {
            PDFDictionary dict = (PDFDictionary)readIndirectObject(pdf, xrefRecord);
            skipOverStreamHeader(pdf.stream);

            return decompressStream(pdf, pdf.stream, dict);
        }

        public static Stream decompressStream(PDF pdf, ObjectReference objectReference)
        {
            PDFDictionary dict = (PDFDictionary)readIndirectObject(pdf, objectReference);
            skipOverStreamHeader(pdf.stream);

            return decompressStream(pdf, pdf.stream, dict);
        }

        public static Stream decompressStream(PDF pdf, int objectNumber, int generation)
        {
            PDFDictionary dict = (PDFDictionary)readIndirectObject(pdf, objectNumber, generation);
            skipOverStreamHeader(pdf.stream);

            return decompressStream(pdf, pdf.stream, dict);
        }

        /// <summary>
        /// decompresses and returns a content stream at the current position
        /// </summary>
        public static Stream decompressStream(PDF pdf, Stream stream, PDFDictionary streamDictionary)
        {
            switch (streamDictionary.get<Name>("Filter"))
            {
                case "FlateDecode":
                    return FlateStreamReader.decompressStream(pdf, stream, streamDictionary);
                default:
                    throw new NotImplementedException();
            }
        }

        public static Bitmap decompressImageStream(PDF pdf, Stream stream, PDFDictionary streamDictionary)
        {
            switch (streamDictionary.get<Name>("Filter"))
            {
                case "FlateDecode":
                    {
                        MemoryStream decompressed = FlateStreamReader.decompressStream(pdf, stream, streamDictionary);
                        byte[] buffer = decompressed.ToArray();
                        
                        return RawStreamReader.convertImageBufferToImage(buffer, streamDictionary);
                    }
                case "DCTDecode":
                    {
                        Stream decompressed = RawStreamReader.decompressStream(stream, streamDictionary);
                        return (Bitmap)Bitmap.FromStream(decompressed);
                    }
                default:
                    throw new NotImplementedException();
            }
        }
        
        public static float readVersion(Stream stream)
        {
            string header = ASCIIReader.readASCIIString(stream, 16);
            string versionString = Regex.Match(header, @"(?<=^%PDF-)\d\.\d").Value;
            return float.Parse(versionString);
        }

        public static Trailer readTrailer(PDF pdf, Stream stream)
        {
            string keyword = ASCIIReader.readASCIIString(stream, 7);
            if (keyword != "trailer")
            {
                throw new Exception("trailer not found at current position");
            }

            skipOverWhiteSpace(stream);
            PDFDictionary dict = readDictionary(pdf, stream);

            return new Trailer(dict);
        }

        public static long readLastStartXREF(string chunk)
        {
            int offset = chunk.LastIndexOf("startxref");
            chunk = chunk.Substring(offset + 9);

            return long.Parse(Regex.Match(chunk, @"(\d+)").Groups[1].Value);
        }

        /// <summary>
        /// returns true if the streams position is currently at an object header
        /// does not modify the position of the stream
        /// </summary>
        public static bool isObjectHeader(Stream stream)
        {
            string start = ASCIIReader.readASCIIString(stream, 32);
            stream.Position -= 32;

            return Regex.IsMatch(start, @"^\d+ \d+ obj");
        }

        /// <summary>
        /// Given a stream whose position is at the start of an indirect object.
        /// This method will skip over the object header and update the streams position to be the first byte of the object.
        /// For example, if the next set of bytes in the stream are:
        ///     1 0 obj\r\n<<...>>
        /// then this method will skip the '1 0 obj\r\n'
        /// if the stream is not positioned at the start of an indirect object then this method will throw an exception
        /// </summary>
        public static void skipOverObjectHeader(Stream stream)
        {
            int objectNumber = ASCIIReader.readASCIIInteger(stream);
            if (stream.ReadByte() != ' ')
            {
                throw new Exception();
            }

            int generation = ASCIIReader.readASCIIInteger(stream);
            if (stream.ReadByte() != ' ')
            {
                throw new Exception();
            }

            string s = ASCIIReader.readASCIIString(stream, 3);
            if (s != "obj")
            {
                throw new Exception();
            }

            skipOverWhiteSpace(stream);
        }
        
        /// <summary>
        /// reads an indirect object from the PDF
        /// </summary>
        public static object readIndirectObject(PDF pdf, ObjectReference objectReference)
        {
            XREFTable.XREFRecord record = pdf.readableTable.getXREFRecord(objectReference);
            return readIndirectObject(pdf, record);
        }

        public static object readIndirectObject(PDF pdf, int objectNumber, int generation)
        {
            XREFTable.XREFRecord record = pdf.readableTable.getXREFRecord(objectNumber, generation);
            return readIndirectObject(pdf, record);
        }

        public static object readIndirectObject(PDF pdf, XREFTable.XREFRecord xrefRecord)
        {
            if (xrefRecord.isCompressed)
            {
                return readCompressedObject(pdf, xrefRecord);
            }

            pdf.stream.Position = xrefRecord.offset;
            skipOverObjectHeader(pdf.stream);

            object obj = readObject(pdf, pdf.stream);
            if (obj is PDFDictionary == false)
            {
                return obj;
            }

            PDFDictionary dict = (PDFDictionary)obj;
            
            if(dict.ContainsKey("Type") || dict.ContainsKey("Subtype"))
            {
                if(dict.ContainsKey("Type") == false)
                {
                    switch (dict.get<Name>("Subtype"))
                    {
                        case "Image":
                        case "Form":
                            dict.set("Type", "XObject");
                            break;
                        default:
                            throw new Exception($"unknown Subtype: " + dict.get<Name>("Subtype"));
                    }
                }

                Name type = dict.get<Name>("Type");

                if (type == "ObjStm")
                {
                    skipOverStreamHeader(pdf.stream);
                    long startOfStream = pdf.stream.Position;

                    return new PDFObjectStream(pdf, dict, startOfStream);
                }
                else if (type == "XObject")
                {
                    skipOverStreamHeader(pdf.stream);
                    long startOfStream = pdf.stream.Position;

                    Name subType = dict.get<Name>("Subtype");

                    switch (subType)
                    {
                        case "Form":
                            return new XObjectForm(pdf, dict, startOfStream);
                        case "Image":
                            return new XObjectImage(pdf, dict, startOfStream);
                        default:
                            throw new Exception($"unknown Subtype: " + subType);
                    }
                }
                else if (type == "Font")
                {
                    return Model.Font.loadExistingFontFromPDF(pdf, dict);
                }
                else
                {
                    return dict;
                }
            }
            else if(dict.ContainsKey("Length"))
            {
                bool isStream = skipOverStreamHeader(pdf.stream);
                if(isStream == false)
                {
                    return dict;
                }
                else
                {
                    return new PDFStream(pdf, dict, pdf.stream.Position);
                }
            }

            return dict;
        }

        /// <summary>
        /// skips over the 'stream keyword and any whitespace surrounding it
        /// returns false if no stream keyword was found
        /// </summary>
        public static bool skipOverStreamHeader(Stream stream)
        {
            skipOverWhiteSpace(stream);

            string chunk = ASCIIReader.readASCIIString(stream, 6);
            if (chunk != "stream")
            {
                return false;
            }

            skipOverWhiteSpace(stream);
            return true;
        }

        private static object readCompressedObject(PDF pdf, XREFTable.XREFRecord xrefRecord)
        {
            PDFObjectStream objectStream = (PDFObjectStream)readIndirectObject(pdf, xrefRecord.compressedObjectNumber, 0);
            return objectStream.readObject(xrefRecord.objectNumber);
        }

        /// <summary>
        /// reads a dictionary from the stream at the current position
        /// </summary>
        public static PDFDictionary readDictionary(PDF pdf, Stream stream)
        {
            PDFDictionary dict = new PDFDictionary(pdf);

            //skip over the <<
            stream.Position += 2;

            skipOverWhiteSpace(stream);

            while (true)
            {
                if (stream.ReadByte() == '>')
                {
                    if (stream.ReadByte() == '>')
                    {
                        return dict;
                    }
                    else
                    {
                        stream.Position -= 2;
                    }
                }
                else
                {
                    stream.Position--;
                }

                string key = readName(stream);
                skipOverWhiteSpace(stream);

                dict.set(key, readObject(pdf, stream));
                skipOverWhiteSpace(stream);
            }
        }

        public static object readObject(PDF pdf, Stream stream)
        {
            char current = (char)stream.ReadByte();
            stream.Position--;

            if (current == '/')
            {
                return readName(stream);
            }
            else if (current == '(')
            {
                return readString(stream);
            }
            else if (current == '[')
            {
                return readArray(pdf, stream);
            }
            else if (current == 't')
            {
                return readBoolean(stream);
            }
            else if (current == '<')
            {
                stream.Position++;
                char current2 = (char)stream.ReadByte();
                stream.Position -= 2;

                if (current2 == '<')
                {
                    return readDictionary(pdf, stream);
                }
                else
                {
                    return readHexString(stream);
                }
            }
            else if (current >= '0' && current <= '9' || ".-".Contains((char)current))
            {
                long currentOffset = stream.Position;
                try
                {
                    return readObjectReference(pdf, stream);
                }
                catch
                {
                    stream.Position = currentOffset;
                }

                return readNumber(stream);
            }
            else
            {
                throw new Exception("unknown object");
            }
        }

        private static object readBoolean(Stream stream)
        {
            string text = ASCIIReader.readASCIIString(stream, 4);
            switch (text)
            {
                case "true":
                    return true;
                case "fals":
                    stream.Position++;
                    return false;
                default:
                    throw new Exception();
            }
        }

        /// <summary>
        /// reads a string (wrapped in brackets) from the stream.
        /// the position of the stream should be at the '('
        /// </summary>
        public static string readString(Stream stream)
        {
            //7.3.4.2

            //skip over the (
            stream.Position++;

            int count = 1;

            StringBuilder sb = new StringBuilder();
            while (true)
            {
                byte current = (byte)stream.ReadByte();
                switch ((char)current)
                {
                    case '\\':
                        current = (byte)stream.ReadByte();
                        switch ((char)current)
                        {
                            case 'n':
                                sb.Append('\n');
                                break;
                            case 'r':
                                sb.Append('\r');
                                break;
                            case 't':
                                sb.Append('\t');
                                break;
                            case 'b':
                                sb.Append('\b');
                                break;
                            case 'f':
                                sb.Append('\f');
                                break;
                            case '(':
                            case ')':
                            case '\\':
                                goto default;
                            case '\r':
                                if (stream.ReadByte() == '\n')
                                {
                                    continue;
                                }
                                else
                                {
                                    stream.Position--;
                                    continue;
                                }
                            case '\n':
                                continue;
                            case '0':
                            case '1':
                            case '2':
                            case '3':
                            case '4':
                            case '5':
                            case '6':
                            case '7':
                            case '8':
                            case '9':
                                int k = current - '0';
                                for (int i = 0; i < 2; i++)
                                {
                                    current = (byte)stream.ReadByte();
                                    if (current < '0' || current > '9')
                                    {
                                        stream.Position--;
                                        break;
                                    }

                                    k *= 8;
                                    k += current - '0';
                                }

                                sb.Append((char)k);
                                break;
                            default:
                                sb.Append((char)current);
                                break;
                        }
                        break;
                    case '(':
                        count++;
                        goto default;
                    case ')':
                        count--;
                        if (count == 0)
                        {
                            return sb.ToString();
                        }
                        else
                        {
                            goto default;
                        }
                    case '\r':
                        sb.Append((char)0x0a);
                        if (stream.ReadByte() != '\n')
                        {
                            stream.Position--;
                        }
                        break;
                    case '\n':
                        sb.Append((char)0x0a);
                        break;
                    default:
                        sb.Append((char)current);
                        break;
                }
            }
        }

        private static byte[] readHexString(Stream stream)
        {
            //skip over the <
            stream.Position++;

            StringBuilder sb = new StringBuilder();
            while (true)
            {
                byte current = (byte)stream.ReadByte();
                switch ((char)current)
                {
                    case '>':
                        return StringToByteArray(sb.ToString());
                    default:
                        sb.Append((char)current);
                        break;
                }
            }
        }

        private static byte[] StringToByteArray(string hex)
        {
            return Enumerable.Range(0, hex.Length)
                             .Where(x => x % 2 == 0)
                             .Select(x => Convert.ToByte(hex.Substring(x, 2), 16))
                             .ToArray();
        }

        /// <summary>
        /// reads an array from the stream. The streams position should be 
        /// at the '['
        /// </summary>
        public static PDFList readArray(PDF pdf, Stream stream)
        {
            //skip over the [
            stream.Position++;

            PDFList array = new PDFList(pdf);

            while (true)
            {
                skipOverWhiteSpace(stream);

                if (stream.ReadByte() == ']')
                {
                    return array;
                }
                else
                {
                    stream.Position--;
                }

                array.add(readObject(pdf, stream));
            }
        }

        /// <summary>
        /// reads a number from the stream
        /// the position of the stream should be the first character of the number
        /// </summary>
        public static object readNumber(Stream stream)
        {
            StringBuilder sb = new StringBuilder();
            while (true)
            {
                byte current = (byte)stream.ReadByte();
                if (current >= '0' && current <= '9')
                {
                    sb.Append((char)current);
                }
                else if (".-".Contains((char)current))
                {
                    sb.Append((char)current);
                }
                else
                {
                    stream.Position--;
                    float f = float.Parse(sb.ToString());
                    if (f == (int)f)
                    {
                        return (int)f;
                    }
                    else
                    {
                        return f;
                    }
                }
            }
        }

        private static ObjectReference readObjectReference(PDF pdf, Stream stream)
        {
            int number = ASCIIReader.readASCIIInteger(stream);

            if (stream.ReadByte() != ' ')
            {
                throw new Exception();
            }

            int generation = ASCIIReader.readASCIIInteger(stream);

            if (stream.ReadByte() != ' ')
            {
                throw new Exception();
            }

            char r = (char)stream.ReadByte();
            if (r != 'R')
            {
                throw new Exception();
            }

            return new ObjectReference(pdf, number, generation);
        }

        /// <summary>
        /// reads a name from the stream.
        /// the position of the stream should be at the '/'
        /// </summary>
        /// <param name="stream"></param>
        /// <returns></returns>
        public static Name readName(Stream stream)
        {
            //skip over the /
            stream.Position++;

            StringBuilder sb = new StringBuilder();

            while (true)
            {
                byte current = (byte)stream.ReadByte();
                if (current >= 'a' && current <= 'z')
                {
                    sb.Append((char)current);
                }
                else if (current >= 'A' && current <= 'Z')
                {
                    sb.Append((char)current);
                }
                else if (current >= '0' && current <= '9')
                {
                    sb.Append((char)current);
                }
                else if (current == '_' || current == '.' || current == '-' || current == '+')
                {
                    sb.Append((char)current);
                }
                else
                {
                    stream.Position--;
                    return new Name(sb.ToString());
                }
            }
        }

        public static void skipOverWhiteSpace(Stream stream)
        {
            while (true)
            {
                switch (stream.ReadByte())
                {
                    case ' ':
                    case '\r':
                    case '\n':
                        break;
                    default:
                        stream.Position--;
                        return;
                }
            }
        }
    }
}
