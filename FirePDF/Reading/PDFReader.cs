using FirePDF.Model;
using FirePDF.StreamHelpers;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace FirePDF.Reading
{
    public static class PdfReader
    {
        public static Stream DecompressStream(Pdf pdf, Stream stream, XrefTable.XrefRecord xrefRecord)
        {
            PdfDictionary dict = (PdfDictionary)ReadIndirectObject(pdf, stream, xrefRecord);
            SkipOverStreamHeader(stream);

            return DecompressStream(pdf, stream, dict);
        }

        /// <summary>
        /// decompresses and returns a content stream at the current position
        /// </summary>
        public static Stream DecompressStream(Pdf pdf, Stream stream, PdfDictionary streamDictionary)
        {
            if(streamDictionary.ContainsKey("Filter") == false)
            {
                return new ProxyStream(stream, stream.Position, streamDictionary.Get<int>("Length"));
            }

            switch (streamDictionary.Get<Name>("Filter"))
            {
                case "FlateDecode":
                    return FlateStreamReader.DecompressStream(pdf, stream, streamDictionary);
                default:
                    throw new NotImplementedException();
            }
        }

        public static Bitmap DecompressImageStream(Pdf pdf, Stream stream, PdfDictionary streamDictionary)
        {
            switch (streamDictionary.Get<Name>("Filter"))
            {
                case "FlateDecode":
                    {
                        MemoryStream decompressed = FlateStreamReader.DecompressStream(pdf, stream, streamDictionary);
                        byte[] buffer = decompressed.ToArray();

                        return RawStreamReader.ConvertImageBufferToImage(buffer, streamDictionary);
                    }
                case "DCTDecode":
                    {
                        Stream decompressed = RawStreamReader.DecompressStream(stream, streamDictionary);
                        return (Bitmap)Image.FromStream(decompressed);
                    }
                default:
                    throw new NotImplementedException();
            }
        }

        public static float ReadVersion(Stream stream)
        {
            string header = AsciiReader.ReadAsciiString(stream, 16);
            string versionString = Regex.Match(header, @"(?<=^%PDF-)\d\.\d").Value;
            return float.Parse(versionString);
        }

        public static Trailer ReadTrailer(Pdf pdf, Stream stream)
        {
            string keyword = AsciiReader.ReadAsciiString(stream, 7);
            if (keyword != "trailer")
            {
                throw new Exception("trailer not found at current position");
            }

            SkipOverWhiteSpace(stream);
            PdfDictionary dict = ReadDictionary(pdf, stream);

            return new Trailer(dict);
        }

        private static long ReadLastStartXref(string chunk)
        {
            int offset = chunk.LastIndexOf("startxref", StringComparison.Ordinal);
            chunk = chunk.Substring(offset + 9);

            return long.Parse(Regex.Match(chunk, @"(\d+)").Groups[1].Value);
        }

        /// <summary>
        /// returns true if the streams position is currently at an object header
        /// does not modify the position of the stream
        /// </summary>
        public static bool IsObjectHeader(Stream stream)
        {
            string start = AsciiReader.ReadAsciiString(stream, 32);
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
        public static void SkipOverObjectHeader(Stream stream)
        {
            int objectNumber = AsciiReader.ReadAsciiInteger(stream);
            if (stream.ReadByte() != ' ')
            {
                throw new Exception();
            }

            int generation = AsciiReader.ReadAsciiInteger(stream);
            if (stream.ReadByte() != ' ')
            {
                throw new Exception();
            }

            string s = AsciiReader.ReadAsciiString(stream, 3);
            if (s != "obj")
            {
                throw new Exception();
            }

            SkipOverWhiteSpace(stream);
        }

        public static long FindLastXrefOffset(Stream stream)
        {
            stream.Position = Math.Max(0, stream.Length - 1024);
            string chunk = AsciiReader.ReadAsciiString(stream, 1024);
            return ReadLastStartXref(chunk);
        }

        /// <summary>
        /// reads the xref tables and trailers from the Pdf
        /// the stream must be set to offset of the last xref table, which can be found by calling findLastXREFOffset()
        /// </summary>
        public static void ReadXrefTables(Pdf pdf, Stream stream, out XrefTable table, out ObjectReference root)
        {
            table = new XrefTable();
            long offsetOfLastXRefTable;
            
            Queue<long> xrefOffsets = new Queue<long>();
            HashSet<long> readOffsets = new HashSet<long>();
            
            xrefOffsets.Enqueue(stream.Position);

            Queue<long> xrefStreams = new Queue<long>();
            root = null;

            while (xrefOffsets.Any())
            {
                stream.Position = xrefOffsets.Dequeue();

                Trailer trailer;
                if (IsObjectHeader(stream))
                {
                    offsetOfLastXRefTable = stream.Position;

                    PdfReader.SkipOverObjectHeader(stream);

                    PdfDictionary dict = PdfReader.ReadDictionary(pdf, stream);
                    PdfReader.SkipOverStreamHeader(stream);

                    XrefStream xrefStream = new XrefStream(stream, dict, stream.Position);
                    
                    table.MergeIn(xrefStream.Table);

                    trailer = xrefStream.Trailer;
                }
                else
                {
                    offsetOfLastXRefTable = stream.Position;
                    XrefTable tempTable = new XrefTable();
                    tempTable.FromStream(stream);

                    table.MergeIn(tempTable);

                    trailer = ReadTrailer(pdf, stream);
                }

                if (root == null)
                {
                    root = trailer.Root;
                }

                if (trailer.Prev.HasValue && readOffsets.Contains(trailer.Prev.Value) == false)
                {
                    xrefOffsets.Enqueue(trailer.Prev.Value);
                }

                if(trailer.XRefStm.HasValue && readOffsets.Contains(trailer.XRefStm.Value) == false)
                {
                    xrefOffsets.Enqueue(trailer.XRefStm.Value);
                }
            }
        }

        public static object ReadIndirectObject(Pdf pdf, Stream stream, XrefTable.XrefRecord xrefRecord)
        {
            if(xrefRecord.objectNumber == 41)
            {
                Console.WriteLine();
            }
            object obj;
            if (xrefRecord.isCompressed)
            {
                obj = ReadCompressedObject(pdf, xrefRecord);
            }
            else
            {
                stream.Position = xrefRecord.offset;
                SkipOverObjectHeader(stream);

                obj = ReadObject(pdf, stream);
            }

            if (obj is PdfDictionary == false)
            {
                return obj;
            }

            PdfDictionary dict = (PdfDictionary)obj;

            if (dict.ContainsKey("Subtype") && !dict.ContainsKey("Type"))
            {
                switch (dict.Get<Name>("Subtype"))
                {
                    case "Image":
                    case "Form":
                        dict.SetWithoutDirtying("Type", (Name)"XObject");
                        break;
                    case "CIDFontType0C":
                    case "Type1C":
                        dict.SetWithoutDirtying("Type", (Name)"Font");
                        break;
                    case "DeviceN": //we can ignore these as we treat color space dictionaries differently
                    case "NChannel":
                        break;
                    default:
                        throw new Exception($"unknown Subtype: " + dict.Get<Name>("Subtype"));
                }
            }

            if (dict.ContainsKey("Length"))
            {
                bool isStream = SkipOverStreamHeader(stream);
                if (isStream)
                {
                    return PdfStream.FromDictionary(dict, stream, stream.Position);
                }
            }

            if (dict.ContainsKey("Type"))
            {
                return HaveUnderlyingDict.FromDictionary(dict);
            }

            return dict;
        }

        /// <summary>
        /// skips over the 'stream keyword and any whitespace surrounding it
        /// returns false if no stream keyword was found
        /// </summary>
        public static bool SkipOverStreamHeader(Stream stream)
        {
            SkipOverWhiteSpace(stream);

            string chunk = AsciiReader.ReadAsciiString(stream, 6);
            if (chunk != "stream")
            {
                return false;
            }

            SkipOverWhiteSpace(stream);
            return true;
        }

        private static object ReadCompressedObject(Pdf pdf, XrefTable.XrefRecord xrefRecord)
        {
            PdfObjectStream objectStream = pdf.Get<PdfObjectStream>(xrefRecord.compressedObjectNumber, 0);
            return objectStream.ReadObject(xrefRecord.objectNumber);
        }

        /// <summary>
        /// reads a dictionary from the stream at the current position
        /// </summary>
        public static PdfDictionary ReadDictionary(Pdf pdf, Stream stream)
        {
            Dictionary<Name, object> dict = new Dictionary<Name, object>();

            //skip over the <<
            stream.Position += 2;

            SkipOverWhiteSpace(stream);

            while (true)
            {
                if (stream.ReadByte() == '>')
                {
                    if (stream.ReadByte() == '>')
                    {
                        return new PdfDictionary(pdf, dict);
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

                string key = ReadName(stream);
                SkipOverWhiteSpace(stream);

                dict.Add(key, ReadObject(pdf, stream));
                SkipOverWhiteSpace(stream);
            }
        }

        public static object ReadObject(Pdf pdf, Stream stream)
        {
            char current = (char)stream.ReadByte();
            stream.Position--;

            if (current == '/')
            {
                return ReadName(stream);
            }
            else if (current == '(')
            {
                return ReadString(stream);
            }
            else if (current == '[')
            {
                return ReadArray(pdf, stream);
            }
            else if (current == 't' || current == 'f')
            {
                return ReadBoolean(stream);
            }
            else if (current == 'n')
            {
                return ReadNull(stream);
            }
            else if (current == '<')
            {
                stream.Position++;
                char current2 = (char)stream.ReadByte();
                stream.Position -= 2;

                if (current2 == '<')
                {
                    return ReadDictionary(pdf, stream);
                }
                else
                {
                    return ReadHexString(stream);
                }
            }
            else if (current >= '0' && current <= '9' || ".-".Contains((char)current))
            {
                long currentOffset = stream.Position;
                if(TryReadObjectReference(pdf, stream, out ObjectReference reference))
                {
                    return reference;
                }
                else
                {
                    stream.Position = currentOffset;
                }

                return ReadNumber(stream);
            }
            else
            {
                string s = AsciiReader.ReadAsciiString(stream, 10);
                throw new Exception("unknown object");
            }
        }

        private static object ReadNull(Stream stream)
        {
            string text = AsciiReader.ReadAsciiString(stream, 4);
            switch (text)
            {
                case "null":
                    return null;
                default:
                    throw new Exception();
            }
        }

        private static bool ReadBoolean(Stream stream)
        {
            string text = AsciiReader.ReadAsciiString(stream, 4);
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
        public static PdfString ReadString(Stream stream)
        {
            //7.3.4.2

            //skip over the (
            stream.Position++;
            long startOfString = stream.Position;

            int count = 1;

            while (count != 0)
            {
                byte current = (byte)stream.ReadByte();
                switch ((char)current)
                {
                    case '\\':
                        //we know that a string doesn't end with a \ so we can simply skip the next character
                        //this nicely handles escaped brackets like '\)'
                        stream.Position++;
                        continue;
                    case '(':
                        count++;
                        continue;
                    case ')':
                        count--;
                        continue;
                }
            }

            int stringLength = (int)(stream.Position - startOfString - 1);
            byte[] bytes = new byte[stringLength];

            stream.Position = startOfString;
            stream.Read(bytes, 0, stringLength);

            //skip over the end bracket
            stream.Position++;

            return new PdfString(bytes, false);
        }

        private static PdfString ReadHexString(Stream stream)
        {
            //skip over the <
            stream.Position++;
            long startOfString = stream.Position;

            StringBuilder sb = new StringBuilder();
            while (true)
            {
                byte current = (byte)stream.ReadByte();
                switch ((char)current)
                {
                    case '>':
                        return new PdfString(StringToByteArray(sb.ToString()), true);
                    default:
                        sb.Append((char)current);
                        break;
                }
            }
        }
        
        private static byte[] StringToByteArray(string hex)
        {
            hex = hex.Replace("\n", "");
            return Enumerable.Range(0, hex.Length)
                             .Where(x => x % 2 == 0)
                             .Select(x => Convert.ToByte(hex.Substring(x, 2), 16))
                             .ToArray();
        }

        /// <summary>
        /// reads an array from the stream. The streams position should be 
        /// at the '['
        /// </summary>
        public static PdfList ReadArray(Pdf pdf, Stream stream)
        {
            //skip over the [
            stream.Position++;

            List<object> list = new List<object>();

            while (true)
            {
                SkipOverWhiteSpace(stream);

                if (stream.ReadByte() == ']')
                {
                    return new PdfList(pdf, list);
                }
                else
                {
                    stream.Position--;
                }

                list.Add(ReadObject(pdf, stream));
            }
        }

        /// <summary>
        /// reads a number from the stream
        /// the position of the stream should be the first character of the number
        /// </summary>
        public static object ReadNumber(Stream stream)
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
                    string temp = sb.ToString();

                    if(temp == "-")
                    {
                        temp = "0";
                    }

                    double f = double.Parse(temp);
                    if (f == (int)f)
                    {
                        return (int)f;
                    }
                    else
                    {
                        return (float)f;
                    }
                }
            }
        }

        private static bool TryReadObjectReference(Pdf pdf, Stream stream, out ObjectReference reference)
        {
            int number = AsciiReader.ReadAsciiInteger(stream);

            if (stream.ReadByte() != ' ')
            {
                reference = null;
                return false;
            }

            int generation = AsciiReader.ReadAsciiInteger(stream);

            if (stream.ReadByte() != ' ')
            {
                reference = null;
                return false;
            }

            char r = (char)stream.ReadByte();
            if (r != 'R')
            {
                reference = null;
                return false;
            }

            reference = new ObjectReference(pdf, number, generation);
            return true;
        }

        private static ObjectReference ReadObjectReferenceOld(Pdf pdf, Stream stream)
        {
            int number = AsciiReader.ReadAsciiInteger(stream);

            if (stream.ReadByte() != ' ')
            {
                throw new Exception();
            }

            int generation = AsciiReader.ReadAsciiInteger(stream);

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
        public static Name ReadName(Stream stream)
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
                else if (current == '_' || current == '.' || current == '-' || current == '+' || current == '*')
                {
                    sb.Append((char)current);
                }
                else if(current == '#')
                {
                    string code = "";
                    code += (char)stream.ReadByte();
                    code += (char)stream.ReadByte();

                    sb.Append((char)Convert.ToByte(code, 16));
                }
                else
                {
                    stream.Position--;
                    return new Name(sb.ToString());
                }
            }
        }

        public static void SkipOverWhiteSpace(Stream stream)
        {
            while (true)
            {
                switch (stream.ReadByte())
                {
                    case 0x09:
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
