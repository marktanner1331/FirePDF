using FirePDF.Model;
using FirePDF.Reading;
using FirePDF.Util;
using FirePDF.Writing;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;

namespace FirePDF.Text
{
    //TODO: change every char to an int
    public class Cmap
    {
        private enum TokenType
        {
            Header,
            Name,
            Operator,
            Value
        }

        public bool IsDirty { get; private set;}

        public readonly int wMode;
        public readonly Name name;
        public readonly int type;
        public readonly string registry;
        public readonly string ordering;
        public readonly int supplement;
        public readonly string version;

        private int minCodeLength = 4;
        private int maxCodeLength = 0;

        private readonly List<CodeSpaceRange> codeSpaceRanges;

        //some codes are mapped to CID's individually and are stored here
        private readonly Dictionary<int, int> cidMap;

        //others are stored as part of CID ranges and are stored here
        private readonly List<CidRange> cidRanges;

        //value must be a string as some codes map to multiple chars (e.g. ligatures)
        private readonly Dictionary<int, string> codeToUnicodeMap;

        public Cmap()
        {
            codeSpaceRanges = new List<CodeSpaceRange>();
            cidMap = new Dictionary<int, int>();
            cidRanges = new List<CidRange>();
            codeToUnicodeMap = new Dictionary<int, string>();
            IsDirty = false;

            //TODO fll these in with default values
            //Maybe best to just load a default empty cmap with all the header and footer stuff sorted?
            throw new NotImplementedException();
        }

        /// <summary>
        /// reads a cmap from the given stream at its current position
        /// comments can come after the 'end' keyword, so this class will try to read to the end of the stream
        /// </summary>
        public Cmap(Stream stream, bool closeStream = false)
        {
            long startOfStream = stream.Position;

            codeSpaceRanges = new List<CodeSpaceRange>();
            cidMap = new Dictionary<int, int>();
            cidRanges = new List<CidRange>();
            codeToUnicodeMap = new Dictionary<int, string>();
            IsDirty = false;

            object previousToken = null;

            while (stream.Position < stream.Length)
            {
                object token = ReadNextToken(stream, out TokenType tokenType);

                switch (tokenType)
                {
                    case TokenType.Name:
                        switch ((Name)token)
                        {
                            case "CMapName":
                                name = (Name)ReadNextToken(stream, out _);
                                break;
                            case "CMapType":
                                type = (int)ReadNextToken(stream, out _);
                                break;
                            case "CMapVersion":
                                version = ReadNextToken(stream, out _).ToString();
                                break;
                            case "Ordering":
                                ordering = ((PdfString)ReadNextToken(stream, out _)).ToString();
                                break;
                            case "Registry":
                                registry = ((PdfString)ReadNextToken(stream, out _)).ToString();
                                break;
                            case "Supplement":
                                supplement = (int)ReadNextToken(stream, out _);
                                break;
                            case "WMode":
                                wMode = (int)ReadNextToken(stream, out _);
                                break;
                        }
                        break;
                    case TokenType.Operator:
                        switch ((string)token)
                        {
                            case "beginbfchar":
                                ReadBeginBfCharFromStream((int)previousToken, stream);
                                break;
                            case "beginbfrange":
                                ReadBeginBfRangeFromStream((int)previousToken, stream);
                                break;
                            case "begincidchar":
                                throw new NotImplementedException();
                            case "begincodespacerange":
                                ReadBeginCodeSpaceRangeFromStream((int)previousToken, stream);
                                break;
                            case "begincidrange":
                                ReadBeginCidRangeFromStream((int)previousToken, stream);
                                break;
                            case "endcmap":
                                goto endOfLoop;
                            case "usecmap":
                                throw new NotImplementedException();
                        }

                        break;
                    case TokenType.Header:
                        break;
                    case TokenType.Value:
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }

                previousToken = token;
            }

            endOfLoop:

            IsDirty = false;
            if(closeStream)
            {
                stream.Close();
                stream.Dispose();
            }
        }

        /// <summary>
        /// reads a named cmap such as /Identity-H
        /// </summary>
        public Cmap(Name cmapName) : this(OpenReadCmapWithName(cmapName), true)
        {
            
        }

        public void addDifferences(PdfList differences)
        {
            throw new NotImplementedException();
        }

        private static Stream OpenReadCmapWithName(Name cmapName)
        {
            //TODO better path resolution
            //even the ability to provide a custom path
            string cmapPath = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) + "/cmaps/" + cmapName;
            if (File.Exists(cmapPath) == false)
            {
                throw new Exception("Unable to find named CMAP: " + cmapName);
            }

            return File.OpenRead(cmapPath);
        }

        private void ReadBeginCodeSpaceRangeFromStream(int numRanges, Stream stream)
        {
            for (int i = 0; i < numRanges; i++)
            {
                object token = ReadNextToken(stream, out TokenType tokenType);

                if (tokenType == TokenType.Operator)
                {
                    if ((string)token != "endcodespacerange")
                    {
                        throw new Exception("found operator inside code space range");
                    }
                    else
                    {
                        break;
                    }
                }

                int start = ((PdfString)token).ToBigEndianInt();
                int codeLength = ((PdfString)token).Length;
                int end = ((PdfString)ReadNextToken(stream, out _)).ToBigEndianInt();

                AddCodeSpaceRange(new CodeSpaceRange(start, end, codeLength));
            }
        }

        private static object ReadNextToken(Stream stream, out TokenType type)
        {
            PdfReader.SkipOverWhiteSpace(stream);
            char current = (char)stream.ReadByte();
            stream.Position--;

            switch (current)
            {
                case '%':
                    type = TokenType.Header;
                    return AsciiReader.ReadLine(stream);
                case '/':
                    type = TokenType.Name;
                    return PdfReader.ReadName(stream);
                case '(':
                case '[':
                case '<':
                    type = TokenType.Value;
                    return PdfReader.ReadObject(null, stream);
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
                    //parsing numbers separately so that the PDFReader doesn't try to parse it as an indirect reference
                    type = TokenType.Value;
                    return PdfReader.ReadNumber(stream);
                default:
                    type = TokenType.Operator;
                    return ReadString(stream);
            }
        }

        private static string ReadString(Stream stream)
        {
            StringBuilder builder = new StringBuilder();

            while (stream.Position != stream.Length)
            {
                char current = (char)stream.ReadByte();

                if (IsWhitespace(current) || "[]<(/".Contains(current) || current >= '0' && current <= '9')
                {
                    stream.Position--;
                    return builder.ToString();
                }

                builder.Append(current);
            }

            return builder.ToString();
        }
       
        private void ReadBeginBfRangeFromStream(int numRows, Stream stream)
        {
            for (int j = 0; j < numRows; j++)
            {
                object token = ReadNextToken(stream, out TokenType tokenType);

                if (tokenType == TokenType.Header)
                {
                    continue;
                }

                if (tokenType == TokenType.Operator)
                {
                    if ((string)token != "endbfrange")
                    {
                        throw new Exception("found operator inside bf range");
                    }
                    else
                    {
                        break;
                    }
                }

                int start = ((PdfString)token).ToBigEndianInt();
                int end = ((PdfString)ReadNextToken(stream, out _)).ToBigEndianInt();

                token = ReadNextToken(stream, out _);

                switch (token)
                {
                    case Name name:
                    {
                        AddCharMapping(start, name);
                        if (start != end)
                        {
                            //if its a name, we can't increment it
                            throw new Exception();
                        }

                        break;
                    }
                    case PdfString pdfStr:
                    {
                        string value = pdfStr.ToString(pdfStr.Length == 1 ? Encoding.GetEncoding("ISO_8859_1") : Encoding.BigEndianUnicode);

                        if (start != end && value.Length > 1)
                        {
                            //we can't increment a value that has multiple characters
                            throw new Exception();
                        }

                        while (true)
                        {
                            AddCharMapping(start, value);

                            if (start == end)
                            {
                                break;
                            }

                            start++;
                            value = ((char)(value[0] + 1)).ToString();
                        }

                        break;
                    }
                    default:
                        throw new Exception("unknown token type");
                }
            }
        }

        private void ReadBeginBfCharFromStream(int numRows, Stream stream)
        {
            for (int j = 0; j < numRows; j++)
            {
                object token = ReadNextToken(stream, out TokenType tokenType);

                if (tokenType == TokenType.Operator)
                {
                    if ((string)token != "endbfchar")
                    {
                        throw new Exception("found operator inside bf char range");
                    }
                    else
                    {
                        break;
                    }
                }

                int code = ((PdfString)token).ToBigEndianInt();

                token = ReadNextToken(stream, out _);

                switch (token)
                {
                    case PdfString pdfStr:
                    {
                        string value;
                        if (pdfStr.Length == 1)
                        {
                            value = pdfStr.ToString(Encoding.GetEncoding("ISO_8859_1"));
                        }
                        else
                        {
                            value = pdfStr.ToString(Encoding.BigEndianUnicode);
                        }

                        AddCharMapping(code, value);
                        break;
                    }
                    case Name name:
                        AddCharMapping(code, name);
                        break;
                    default:
                        throw new Exception("error reading beginbfchar, unknown token: " + token);
                }
            }
        }

        private void ReadBeginCidRangeFromStream(int numRows, Stream stream)
        {
            for (int i = 0; i < numRows; i++)
            {
                object token = ReadNextToken(stream, out TokenType tokenType);

                if (tokenType == TokenType.Operator)
                {
                    if ((string)token != "endcidrange")
                    {
                        throw new Exception("found operator inside CID range");
                    }
                    break;
                }

                int start = ((PdfString)token).ToBigEndianInt();
                int end = ((PdfString)ReadNextToken(stream, out _)).ToBigEndianInt();

                int mappedCode = (int)ReadNextToken(stream, out _);

                //when the start equals the end it means that the range is being used to map a single code
                if (end == start)
                {
                    AddCidMapping((char)start, mappedCode);
                }
                else
                {
                    AddCidRange((char)start, (char)end, mappedCode);
                }
            }
        }

        /// <summary>
        /// reads a big endian int from the start of the buffer, consuming 'length' bytes
        /// </summary>
        private static int ReadBigEndianInt(byte[] buffer, int length)
        {
            int code = 0;
            
            for (int i = 0; i < length; ++i)
            {
                code <<= 8;
                code |= buffer[i] & 0xFF;
            }

            return code;
        }

        private static bool IsWhitespace(char c)
        {
            switch (c)
            {
                case (char)0:
                case (char)9:
                case (char)12:
                case '\r':
                case '\n':
                case ' ':
                    return true;
                default:
                    return false;
            }
        }

        public string CodeToUnicode(int code)
        {
            return codeToUnicodeMap.ContainsKey(code) ? codeToUnicodeMap[code] : null;
        }

        public int CodeToCid(int code)
        {
            if(cidMap.ContainsKey(code))
            {
                return cidMap[code];
            }

            return cidRanges.Select(range => range.CodeToCid(code)).FirstOrDefault(cid => cid != -1);
        }

        /// <summary>
        /// reads a character code from the stream
        /// </summary>
        public int ReadCodeFromStream(Stream stream)
        {
            //we create a buffer that stores our code
            byte[] bytes = new byte[maxCodeLength];

            //we only read in minCodeLength - 1 bytes at the moment
            //as each iteration of the below loop will read in one more byte as the size of 'codeLength' increases
            stream.Read(bytes, 0, minCodeLength - 1);
            
            for (int codeLength = minCodeLength; codeLength <= maxCodeLength; codeLength++)
            {
                //if codeLength is 4 then we have read in the first 3 bytes already
                //meaning we need to read in the 4th byte
                //which has position '3' in the array
                //i.e. codeLength - 1
                bytes[codeLength - 1] = (byte)stream.ReadByte();

                foreach (CodeSpaceRange range in codeSpaceRanges)
                {
                    if (range.codeLength != codeLength) continue;
                    int code = ReadBigEndianInt(bytes, codeLength);
                    if (range.IsInRange(code))
                    {
                        return code;
                    }
                }
            }

            Logger.Warning("Invalid character code sequence: " + ByteWriter.ByteArrayToHexString(bytes) + "in CMap: " + name);
            return 0;
        }

        public void WriteToStream(Stream stream)
        {
            throw new NotImplementedException();
        }

        public void AddCodeSpaceRange(CodeSpaceRange range)
        {
            codeSpaceRanges.Add(range);
            maxCodeLength = Math.Max(maxCodeLength, range.codeLength);
            minCodeLength = Math.Min(minCodeLength, range.codeLength);
            IsDirty = true;
        }

        public void AddCidMapping(char code, int cid)
        {
            cidMap[code] = cid;
            IsDirty = true;
        }

        /// <summary>
        /// adds the given CID range to the set of CID ranges covered by this CMAP
        /// </summary>
        public void AddCidRange(char start, char end, int cid)
        {
            //first we check if the most recent range can be extended to cover the new range
            if(cidRanges.Count != 0)
            {
                if(cidRanges.Last().TryExtend(start, end, cid))
                {
                    return;
                }
            }

            //otherwise add a new range
            cidRanges.Add(new CidRange(start, end, cid));
            IsDirty = true;
        }

        public void AddCharMapping(int code, string unicode)
        {
            codeToUnicodeMap.Add(code, unicode);
            IsDirty = true;
        }
    }
}
