using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace FirePDF
{
    public static class PDFObjectReader
    {
        public static PDFContentStream readContentStream(PDF pdf, ObjectReference objectReference)
        {
            Dictionary<string, object> dict = readIndirectDictionary(pdf, objectReference);
            skipOverWhiteSpace(pdf.stream);
            long startOfStream = pdf.stream.Position;

            switch((string)dict["Filter"])
            {
                case "FlateDecode":
                    return new FlateContentStream(pdf, dict, startOfStream);
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

        public static void skipOverObjectHeader(Stream stream)
        {
            int objectNumber = FileReader.readASCIIInteger(stream);
            if(stream.ReadByte() != ' ')
            {
                throw new Exception();
            }

            int generation = FileReader.readASCIIInteger(stream);
            if(stream.ReadByte() != ' ')
            {
                throw new Exception();
            }

            string s = FileReader.readASCIIString(stream, 3);
            if(s != "obj")
            {
                throw new Exception();
            }

            skipOverWhiteSpace(stream);
        }

        public static Dictionary<string, object> readIndirectDictionary(PDF pdf, ObjectReference objectReference)
        {
            return (Dictionary<string, object>)readIndirectObject(pdf, objectReference);
        }

        /// <summary>
        /// reads an indirect object from the PDF
        /// </summary>
        public static object readIndirectObject(PDF pdf, ObjectReference objectReference)
        {
            pdf.stream.Position = pdf.readTable.getOffsetForRecord(objectReference.objectNumber, objectReference.generation);
            skipOverObjectHeader(pdf.stream);
            return readObject(pdf.stream);
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
            else if(current == '(')
            {
                return readString(stream);
            }
            else if(current == '[')
            {
                return readArray(stream);
            }
            else if(current == 't')
            {
                return readBoolean(stream);
            }
            else if(current == '<')
            {
                stream.Position++;
                char current2 = (char)stream.ReadByte();
                stream.Position -= 2;

                if(current2 == '<')
                {
                    return readDictionary(stream);
                }
                else
                {
                    return readHexString(stream);
                }
            }
            else if (current >= '0' && current <= '9')
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
            string text = FileReader.readASCIIString(stream, 4);
            switch(text)
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
        /// <param name="stream"></param>
        /// <returns></returns>
        public static object readString(Stream stream)
        {
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
                        throw new Exception();
                        break;
                    case '(':
                        count++;
                        goto default;
                    case ')':
                        count--;
                        if(count == 0)
                        {
                            return sb.ToString();
                        }
                        else
                        {
                            goto default;
                        }
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

            while(true)
            {
                array.Add(readObject(stream));

                skipOverWhiteSpace(stream);

                if(stream.ReadByte() == ']')
                {
                    return array;
                }
                else
                {
                    stream.Position--;
                }
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
                else if(".-".Contains((char)current))
                {
                    sb.Append((char)current);
                }
                else
                {
                    stream.Position--;
                    float f = float.Parse(sb.ToString());
                    if(f == (int)f)
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
            int number = FileReader.readASCIIInteger(stream);
            
            if(stream.ReadByte() != ' ')
            {
                throw new Exception();
            }

            int generation = FileReader.readASCIIInteger(stream);

            if (stream.ReadByte() != ' ')
            {
                throw new Exception();
            }

            char r = (char)stream.ReadByte();
            if(r != 'R')
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

            while(true)
            {
                byte current = (byte)stream.ReadByte();
                if(current >= 'a' && current <= 'z')
                {
                    sb.Append((char)current);
                }
                else if (current >= 'A' && current <= 'Z')
                {
                    sb.Append((char)current);
                }
                else if(current >='0' && current <= '9')
                {
                    sb.Append((char)current);
                }
                else if(current == '_')
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
