using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FirePDF
{
    public static class ContentStreamReader
    {
        public static List<Operation> readContentStream(Stream decompressedStream)
        {
            List<Operation> operations = new List<Operation>();
            Operation currentOperation = new Operation();

            readTokens(
                decompressedStream,
                operatorName =>
                {
                    currentOperation.operatorName = operatorName;
                    operations.Add(currentOperation);
                    currentOperation = new Operation();
                },
                operand => currentOperation.operands.Add(operand));

            return operations;
        }

        private static void readTokens(Stream stream, Action<string> foundOperator, Action<object> foundOperand)
        {
            while (stream.Position < stream.Length)
            {
                skipOverWhiteSpace(stream);
                char current = (char)stream.ReadByte();
                stream.Position--;

                switch (current)
                {
                    case '<': //either start of dict or hex string
                        {
                            object obj = PDFObjectReader.readObject(stream);
                            foundOperand(obj);
                        }
                        break;
                    case '[':
                        {
                            object obj = PDFObjectReader.readArray(stream);
                            foundOperand(obj);
                        }
                        break;
                    case '(':
                        {
                            object obj = PDFObjectReader.readString(stream);
                            foundOperand(obj);
                        }
                        break;
                    case '/':
                        {
                            object obj = PDFObjectReader.readName(stream);
                            foundOperand(obj);
                        }
                        break;
                    case 'B': //inline images
                        {
                            string operatorName = readString(stream);
                            if (operatorName == "BI")
                            {
                                stream.Position -= 2;
                                List<object> data = readInlineImage(stream);
                                data.ForEach(foundOperand);
                                foundOperator("BI");
                            }
                            else
                            {
                                foundOperator(operatorName);
                            }
                        }
                        break;
                    case 'n':
                        {
                            string s = readString(stream);
                            if (s == "null")
                            {
                                //need to check im doing this right
                                //if we parse out null then i think its an operand
                                //but im not completely sure
                                //it could be a nop
                                //and would therefore be an operator, with no operands
                                throw new NotImplementedException();
                                foundOperand(null);
                            }
                            else
                            {
                                foundOperator(s);
                            }
                        }
                        break;
                    case 'f':
                        {
                            string s = readString(stream);
                            if (s == "false")
                            {
                                foundOperand(false);
                            }
                            else
                            {
                                foundOperator(s);
                            }
                        }
                        break;
                    case 't':
                    case 'R':
                    case 'I':
                    case ']':
                        throw new NotImplementedException("token not supported: " + current);
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
                    case '-':
                    case '+':
                    case '.':
                        {
                            object obj = PDFObjectReader.readNumber(stream);
                            foundOperand(obj);
                        }
                        break;
                    default: //if it matches none of the above cases then it must be an operator
                        {
                            string operatorName = readString(stream);
                            foundOperator(operatorName);
                        }
                        break;
                }
            }
        }

        private static List<object> readInlineImage(Stream stream)
        {
            while (true)
            {
                while (stream.ReadByte() != 'E') { }
                if (stream.ReadByte() == 'I')
                {
                    return new List<object>();
                }
                else
                {
                    stream.Position--;
                }
            }
        }

        private static string readString(Stream stream)
        {
            skipOverWhiteSpace(stream);

            //average string size is around 2 and the normal string buffer size is
            //about 16 so lets save some space.
            StringBuilder builder = new StringBuilder(4);
            char previous = (char)0;

            while (true)
            {
                char current = (char)stream.ReadByte();

                if (isWhitespace(current) || "[]<(/".Contains(current))
                {
                    return builder.ToString();
                }
                else if (current >= '0' && current <= '9')
                {
                    //operators for type3 glyphs can have numbers in their names annoyingly
                    if (previous == 'd' && (current == '0' || current == '1'))
                    {
                        goto pushChar;
                    }
                    else
                    {
                        return builder.ToString();
                    }
                }

            pushChar:;
                builder.Append(current);
                previous = current;
            }
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

        private static void skipOverWhiteSpace(Stream stream)
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
