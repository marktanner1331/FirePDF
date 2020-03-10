using FirePDF.Reading;
using FirePDF.Util;
using FirePDF.Writing;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace FirePDF.Model
{
    //TODO: change every char to an int
    public class CMAP
    {
        private enum TokenType
        {
            Header,
            Name,
            Operator,
            Value
        }

        public bool isDirty { get; private set;}

        public readonly int wMode;
        public readonly Name name;
        public readonly int type;
        public readonly string registry;
        public readonly string ordering;
        public readonly int supplement;
        public readonly string version;

        private int minCodeLength = 4;
        private int maxCodeLength = 0;

        private List<CodeSpaceRange> codeSpaceRanges;

        //some codes are mapped to CID's individually and are stored here
        private Dictionary<int, int> cidMap;

        //others are stored as part of CID ranges and are stored here
        private List<CIDRange> cidRanges;

        //value must be a string as some codes map to multiple chars (e.g. ligatures)
        private Dictionary<int, string> codeToUnicodeMap;

        public CMAP()
        {
            codeSpaceRanges = new List<CodeSpaceRange>();
            cidMap = new Dictionary<int, int>();
            cidRanges = new List<CIDRange>();
            codeToUnicodeMap = new Dictionary<int, string>();

            //TODO fll these in with default values
            //Maybe best to just load a default empty cmap with all the header and footer stuff sorted?
            throw new NotImplementedException();
            isDirty = false;
        }

        /// <summary>
        /// reads a cmap from the given stream at its current position
        /// comments can come after the 'end' keyword, so this class will try to read to the end of the stream
        /// </summary>
        public CMAP(Stream stream, bool closeStream = false)
        {
            long startOfStream = stream.Position;

            codeSpaceRanges = new List<CodeSpaceRange>();
            cidMap = new Dictionary<int, int>();
            cidRanges = new List<CIDRange>();
            codeToUnicodeMap = new Dictionary<int, string>();
            isDirty = false;

            object previousToken = null;
            object token;
            TokenType tokenType;

            while (stream.Position < stream.Length)
            {
                token = readNextToken(stream, out tokenType);

                switch (tokenType)
                {
                    case TokenType.Name:
                        switch ((Name)token)
                        {
                            case "CMapName":
                                name = (Name)readNextToken(stream, out _);
                                break;
                            case "CMapType":
                                type = (int)readNextToken(stream, out _);
                                break;
                            case "CMapVersion":
                                version = readNextToken(stream, out _).ToString();
                                break;
                            case "Ordering":
                                ordering = ((PDFString)readNextToken(stream, out _)).ToString();
                                break;
                            case "Registry":
                                registry = ((PDFString)readNextToken(stream, out _)).ToString();
                                break;
                            case "Supplement":
                                supplement = (int)readNextToken(stream, out _);
                                break;
                            case "WMode":
                                wMode = (int)readNextToken(stream, out _);
                                break;
                        }
                        break;
                    case TokenType.Operator:
                        switch ((string)token)
                        {
                            case "beginbfchar":
                                readBeginBFCharFromStream((int)previousToken, stream);
                                break;
                            case "beginbfrange":
                                readBeginBFRangeFromStream((int)previousToken, stream);
                                break;
                            case "begincidchar":
                                throw new NotImplementedException();
                            case "begincodespacerange":
                                readBeginCodeSpaceRangeFromStream((int)previousToken, stream);
                                break;
                            case "begincidrange":
                                readBeginCIDRangeFromStream((int)previousToken, stream);
                                break;
                            case "endcmap":
                                goto endOfLoop;
                            case "usecmap":
                                throw new NotImplementedException();
                        }

                        break;
                }

                previousToken = token;
            }

            endOfLoop:

            isDirty = false;
            if(closeStream)
            {
                stream.Close();
                stream.Dispose();
            }
        }

        /// <summary>
        /// reads a named cmap such as /Identity-H
        /// </summary>
        public CMAP(Name cmapName) : this(openReadCmapWithName(cmapName), true)
        {
            
        }

        private static Stream openReadCmapWithName(Name cmapName)
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

        private void readBeginCodeSpaceRangeFromStream(int numRanges, Stream stream)
        {
            for (int i = 0; i < numRanges; i++)
            {
                TokenType tokenType;
                object token = readNextToken(stream, out tokenType);

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

                int start = ((PDFString)token).toBigEndianInt();
                int codeLength = ((PDFString)token).length;
                int end = ((PDFString)readNextToken(stream, out _)).toBigEndianInt();

                addCodeSpaceRange(new CodeSpaceRange(start, end, codeLength));
            }
        }

        private static object readNextToken(Stream stream, out TokenType type)
        {
            PDFReader.skipOverWhiteSpace(stream);
            char current = (char)stream.ReadByte();
            stream.Position--;

            switch (current)
            {
                case '%':
                    type = TokenType.Header;
                    return ASCIIReader.readLine(stream);
                case '/':
                    type = TokenType.Name;
                    return PDFReader.readName(stream);
                case '(':
                case '[':
                case '<':
                    type = TokenType.Value;
                    return PDFReader.readObject(null, stream);
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
                    //parsing numbers seperately so that the PDFReader doesnt try to parse it as an indirect reference
                    type = TokenType.Value;
                    return PDFReader.readNumber(stream);
                default:
                    type = TokenType.Operator;
                    return readString(stream);
            }
        }

        private static string readString(Stream stream)
        {
            StringBuilder builder = new StringBuilder();

            while (stream.Position != stream.Length)
            {
                char current = (char)stream.ReadByte();

                if (isWhitespace(current) || "[]<(/".Contains(current) || (current >= '0' && current <= '9'))
                {
                    stream.Position--;
                    return builder.ToString();
                }

                builder.Append(current);
            }

            return builder.ToString();
        }
       
        private void readBeginBFRangeFromStream(int numRows, Stream stream)
        {
            for (int j = 0; j < numRows; j++)
            {
                TokenType tokenType;
                object token = readNextToken(stream, out tokenType);

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

                int start = ((PDFString)token).toBigEndianInt();
                int end = ((PDFString)readNextToken(stream, out _)).toBigEndianInt();

                token = readNextToken(stream, out _);

                if (token is Name)
                {
                    addCharMapping(start, (Name)token);
                    if (start != end)
                    {
                        //if its a name, we can't increment it
                        throw new Exception();
                    }
                }
                else if (token is PDFString pdfStr)
                {
                    string value;
                    if (pdfStr.length == 1)
                    {
                        value = pdfStr.toString(Encoding.GetEncoding("ISO_8859_1"));
                    }
                    else
                    {
                        value = pdfStr.toString(Encoding.BigEndianUnicode);
                    }

                    if (start != end && value.Length > 1)
                    {
                        //we can't increment a value that has multiple characters
                        throw new Exception();
                    }

                    while (true)
                    {
                        addCharMapping(start, value);

                        if (start == end)
                        {
                            break;
                        }

                        start++;
                        value = ((char)(value[0] + 1)).ToString(); ;
                    }
                }
                else
                {
                    throw new Exception("unknown token type");
                }
            }
        }

        private void readBeginBFCharFromStream(int numRows, Stream stream)
        {
            for (int j = 0; j < numRows; j++)
            {
                TokenType tokenType;
                object token = readNextToken(stream, out tokenType);

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

                int code = ((PDFString)token).toBigEndianInt();

                token = readNextToken(stream, out _);

                if (token is PDFString pdfStr)
                {
                    string value;
                    if (pdfStr.length == 1)
                    {
                        value = pdfStr.toString(Encoding.GetEncoding("ISO_8859_1"));
                    }
                    else
                    {
                        value = pdfStr.toString(Encoding.BigEndianUnicode);
                    }

                    addCharMapping(code, value);
                }
                else if (token is Name)
                {
                    addCharMapping(code, (Name)token);
                }
                else
                {
                    throw new Exception("error reading beginbfchar, unknown token: " + token);
                }
            }
        }

        private void readBeginCIDRangeFromStream(int numRows, Stream stream)
        {
            for (int i = 0; i < numRows; i++)
            {
                TokenType tokenType;
                object token = readNextToken(stream, out tokenType);

                if (tokenType == TokenType.Operator)
                {
                    if ((string)token != "endcidrange")
                    {
                        throw new Exception("found operator inside CID range");
                    }
                    break;
                }

                int start = ((PDFString)token).toBigEndianInt();
                int end = ((PDFString)readNextToken(stream, out _)).toBigEndianInt();

                int mappedCode = (int)readNextToken(stream, out _);

                //when the start equals the end it means that the range is being used to map a single code
                if (end == start)
                {
                    addCIDMapping((char)start, mappedCode);
                }
                else
                {
                    addCIDRange((char)start, (char)end, mappedCode);
                }
            }
        }

        /// <summary>
        /// reads a big endian int from the start of the buffer, consuming 'length' bytes
        /// </summary>
        private static int readBigEndianInt(byte[] buffer, int length)
        {
            int code = 0;

            for (int i = 0; i < length; ++i)
            {
                code <<= 8;
                code |= (buffer[i] & 0xFF);
            }

            return code;
        }

        private static bool isWhitespace(char c)
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

        public string codeToUnicode(int code)
        {
            if(codeToUnicodeMap.ContainsKey(code))
            {
                return codeToUnicodeMap[code];
            }

            return null;
        }

        public int codeToCID(int code)
        {
            if(cidMap.ContainsKey(code))
            {
                return cidMap[code];
            }
            
            foreach (CIDRange range in cidRanges)
            {
                int cid = range.codeToCID(code);
                if (cid != -1)
                {
                    return cid;
                }
            }

            return 0;
        }

        /// <summary>
        /// reads a character code from the stream
        /// </summary>
        public int readCodeFromStream(Stream stream)
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
                    if (range.codeLength == codeLength)
                    {
                        int code = readBigEndianInt(bytes, codeLength);
                        if (range.isInRange(code))
                        {
                            return code;
                        }
                    }
                }
            }

            Logger.warning("Invalid character code sequence: " + ByteWriter.byteArrayToHexString(bytes) + "in CMap: " + name);
            return 0;
        }

        public void writeToStream(Stream stream)
        {
            throw new NotImplementedException();
        }

        public void addCodeSpaceRange(CodeSpaceRange range)
        {
            codeSpaceRanges.Add(range);
            maxCodeLength = Math.Max(maxCodeLength, range.codeLength);
            minCodeLength = Math.Min(minCodeLength, range.codeLength);
            isDirty = true;
        }

        public void addCIDMapping(char code, int cid)
        {
            cidMap[code] = cid;
            isDirty = true;
        }

        /// <summary>
        /// adds the given CID range to the set of CID ranges covered by this CMAP
        /// </summary>
        public void addCIDRange(char start, char end, int cid)
        {
            //first we check if the most recent range can be extended to cover the new range
            if(cidRanges.Count != 0)
            {
                if(cidRanges.Last().tryExtend(start, end, cid))
                {
                    return;
                }
            }

            //otherwise add a new range
            cidRanges.Add(new CIDRange(start, end, cid));
            isDirty = true;
        }

        public void addCharMapping(int code, string unicode)
        {
            codeToUnicodeMap.Add(code, unicode);
            isDirty = true;
        }
    }
}
