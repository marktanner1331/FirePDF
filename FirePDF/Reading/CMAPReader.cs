using FirePDF.Model;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace FirePDF.Reading
{
    public static class CMAPReader
    {
        public enum TokenType
        {
            Header,
            Name,
            Operator,
            Value
        }

        public static CMAP readCMAP(Stream stream)
        {
            CMAP cmap = new CMAP();

            Object previousToken = null;
            Object token;
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
                                ;
                                cmap.name = (Name)readNextToken(stream, out _);
                                break;
                            case "CMapType":
                                cmap.type = (int)readNextToken(stream, out _);
                                break;
                            case "CMapVersion":
                                cmap.version = readNextToken(stream, out _).ToString();
                                break;
                            case "Ordering":
                                cmap.ordering = (string)readNextToken(stream, out _);
                                break;
                            case "Registry":
                                cmap.registry = (string)readNextToken(stream, out _);
                                break;
                            case "Supplement":
                                cmap.supplement = (int)readNextToken(stream, out _);
                                break;
                            case "WMode":
                                cmap.wMode = (int)readNextToken(stream, out _);
                                break;
                        }
                        break;
                    case TokenType.Operator:
                        switch ((string)token)
                        {

                            case "beginbfchar":
                                readBeginBFChar((int)previousToken, stream, cmap);
                                break;
                            case "beginbfrange":
                                throw new NotImplementedException();
                            case "begincidchar":
                                throw new NotImplementedException();
                            case "begincodespacerange":
                                readBeginCodeSpaceRange((int)previousToken, stream, cmap);
                                break;
                            case "begincidrange":
                                readBeginCIDRange((int)previousToken, stream, cmap);
                                break;
                            case "endcmap":
                                return cmap;
                            case "usecmap":
                                throw new NotImplementedException();
                        }
                        break;
                }

                previousToken = token;
            }

            return cmap;
        }
        
        public static CMAP readNamedCMAP(string cmapName)
        {
            string cmapPath = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) + "/cmaps/" + cmapName;
            if (File.Exists(cmapPath) == false)
            {
                throw new Exception("Unable to find named CMAP: " + cmapName);
            }

            using (Stream stream = File.OpenRead(cmapPath))
            {
                return readCMAP(stream);
            }
        }

        private static void readBeginBFChar(int numRows, Stream stream, CMAP cmap)
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
                
                byte[] codeBuffer = (byte[])token;
                int code = ByteReader.readBigEndianInt(codeBuffer);

                token = readNextToken(stream, out _);

                if (token is byte[])
                {
                    byte[] bytes = (byte[])token;
                    string value;

                    if(bytes.Length == 1)
                    {
                        value = Encoding.GetEncoding("ISO_8859_1").GetString(bytes);
                    }
                    else
                    {
                        value = Encoding.BigEndianUnicode.GetString(bytes);
                    }
                    
                    cmap.addCharMapping(code, value);
                }
                else if (token is Name)
                {
                    cmap.addCharMapping(code, (Name)token);
                }
                else
                {
                    throw new Exception("error reading beginbfchar, unknown token: " + token);
                }
            }
        }

        private static void readBeginCodeSpaceRange(int numRanges, Stream stream, CMAP cmap)
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

                int start = ByteReader.readBigEndianInt((byte[])token);
                int codeLength = ((byte[])token).Length;
                int end = ByteReader.readBigEndianInt((byte[])readNextToken(stream, out _));

                cmap.addCodeSpaceRange(new CodeSpaceRange(start, end, codeLength));
            }
        }

        private static void readBeginCIDRange(int numRows, Stream stream, CMAP cmap)
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

                byte[] startCode = (byte[])token;
                int start = ByteReader.readBigEndianInt(startCode);

                byte[] endCode = (byte[])readNextToken(stream, out _);
                int end = ByteReader.readBigEndianInt(endCode);

                int mappedCode = (int)readNextToken(stream, out _);

                if (startCode.Length <= 2 && endCode.Length <= 2)
                {
                    //when the start equals the end it means that the range is being used to map a single code
                    if (end == start)
                    {
                        cmap.addCIDMapping((char)start, mappedCode);
                    }
                    else
                    {
                        cmap.addCIDRange((char)start, (char)end, mappedCode);
                    }
                }
                else
                {
                    throw new Exception();
                }
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
    }
}
