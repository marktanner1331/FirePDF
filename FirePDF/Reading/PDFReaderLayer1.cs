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
    public static class PDFReaderLayer1
    {
        public static PDFContentStream readContentStream(PDF pdf, XREFTable.XREFRecord xrefRecord)
        {
            Dictionary<string, object> dict = readIndirectDictionary(pdf, xrefRecord);
            skipOverWhiteSpace(pdf.stream);
            long startOfStream = pdf.stream.Position;

            return readContentStream(pdf, dict, startOfStream);
        }

        public static PDFContentStream readContentStream(PDF pdf, ObjectReference objectReference)
        {
            Dictionary<string, object> dict = readIndirectDictionary(pdf, objectReference);
            skipOverWhiteSpace(pdf.stream);
            long startOfStream = pdf.stream.Position;

            return readContentStream(pdf, dict, startOfStream);
        }

        public static PDFContentStream readContentStream(PDF pdf, int objectNumber, int generation)
        {
            Dictionary<string, object> dict = readIndirectDictionary(pdf, objectNumber, generation);
            skipOverWhiteSpace(pdf.stream);
            long startOfStream = pdf.stream.Position;

            return readContentStream(pdf, dict, startOfStream);
        }

        public static PDFContentStream readContentStream(PDF pdf, Dictionary<string, object> streamDictionary, long startOfStream)
        {
            switch ((string)streamDictionary["Filter"])
            {
                case "FlateDecode":
                    return new FlateContentStream(pdf, streamDictionary, startOfStream);
                default:
                    throw new NotImplementedException();
            }
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

        public static Dictionary<string, object> readIndirectDictionary(PDF pdf, ObjectReference objectReference)
        {
            return (Dictionary<string, object>)readIndirectObject(pdf, objectReference);
        }

        public static Dictionary<string, object> readIndirectDictionary(PDF pdf, XREFTable.XREFRecord xrefRecord)
        {
            return (Dictionary<string, object>)readIndirectObject(pdf, xrefRecord);
        }

        public static Dictionary<string, object> readIndirectDictionary(PDF pdf, int objectNumber, int generation)
        {
            return (Dictionary<string, object>)readIndirectObject(pdf, objectNumber, generation);
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

            object obj = readObject(pdf.stream);
            if(obj is Dictionary<string, object> == false)
            {
                return obj;
            }

            Dictionary<string, object> dict = (Dictionary<string, object>)obj;
            
            if(dict.ContainsKey("Subtype") == false)
            {
                return dict;
            }

            skipOverWhiteSpace(pdf.stream);
            long startOfStream = pdf.stream.Position;

            if ((string)dict["Subtype"] == "Form")
            {
                return new XObjectForm(pdf, dict, startOfStream);
            }
            else if ((string)dict["Subtype"] == "Image")
            {
                return new XObjectImage(pdf, dict, startOfStream);
            }
            else
            {
                throw new Exception($"unknown Subtype: " + dict["Subtype"]);
            }
        }

        private static object readCompressedObject(PDF pdf, XREFTable.XREFRecord xrefRecord)
        {
            PDFObjectStream objectStream = new PDFObjectStream(pdf, xrefRecord.compressedObjectNumber);
            return objectStream.readObject(xrefRecord.objectNumber);
        }

        /// <summary>
        /// reads a dictionary from the stream at the current position
        /// </summary>
        public static Dictionary<string, object> readDictionary(Stream stream)
        {
            Dictionary<string, object> dict = new Dictionary<string, object>();

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

                dict[key] = readObject(stream);
                skipOverWhiteSpace(stream);
            }
        }

        public static object readObject(Stream stream)
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
                return readArray(stream);
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
                    return readDictionary(stream);
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
                    return readObjectReference(stream);
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
        public static List<object> readArray(Stream stream)
        {
            //skip over the [
            stream.Position++;

            List<object> array = new List<object>();

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

                array.Add(readObject(stream));
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

        private static ObjectReference readObjectReference(Stream stream)
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

            return new ObjectReference(number, generation);
        }

        /// <summary>
        /// reads a name from the stream.
        /// the position of the stream should be at the '/'
        /// </summary>
        /// <param name="stream"></param>
        /// <returns></returns>
        public static string readName(Stream stream)
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
                else if (current == '_' || current == '.')
                {
                    sb.Append((char)current);
                }
                else
                {
                    stream.Position--;
                    return sb.ToString();
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
