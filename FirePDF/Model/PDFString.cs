using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace FirePDF.Model
{
    public class PdfString
    {
        private readonly byte[] bytes;
        private readonly string value;
        public readonly bool isHexString;

        /// <summary>
        /// returns the number of bytes used to store this string
        /// for example, the length of <00FF> is 2
        /// </summary>
        public int Length => bytes.Length;

        public PdfString(byte[] bytes)
        {
            this.bytes = bytes;
            isHexString = true;
            value = Encoding.ASCII.GetString(bytes);
        }

        public PdfString(byte[] bytes, bool isHexString)
        {
            this.bytes = bytes;
            this.isHexString = isHexString;
            value = Encoding.ASCII.GetString(bytes);
        }

        public PdfString(string value) : this(ReadFromStringLiteral(value), false)  {}

        /// <summary>
        /// converts a string literal, usually surrounded by () in the pdf file, into a byte array, parsing any escaped characters
        /// </summary>
        /// <param name="value">the string literal, without its enclosing brackets</param>
        private static byte[] ReadFromStringLiteral(string value)
        {
            List<byte> temp = new List<byte>();

            for (int i = 0; i < value.Length; i++)
            {
                switch (value[i])
                {
                    case '\\':
                        switch (value[++i])
                        {
                            case 'n':
                                temp.Add((byte)'\n');
                                break;
                            case 'r':
                                temp.Add((byte)'\r');
                                break;
                            case 't':
                                temp.Add((byte)'\t');
                                break;
                            case 'b':
                                temp.Add((byte)'\b');
                                break;
                            case 'f':
                                temp.Add((byte)'\f');
                                break;
                            case '(':
                            case ')':
                            case '\\':
                                goto default;
                            case '\r':
                                //a backslash followed by a new line means its a single line string split over multiple lines
                                //this means we can simply skip over these bytes
                                //and act like they were never there
                                if (value[i + 1] == '\n')
                                {
                                    i++;
                                }
                                continue;
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
                                //we have an octal character
                                int k = ((value[i] - '0') << 6) + ((value[i + 1] - '0') << 3) + (value[i + 2] - '0');
                                i += 2;

                                temp.Add((byte)k);
                                break;
                            default:
                                temp.Add((byte)value[i]);
                                break;
                        }
                        break;
                    case '\r':
                        temp.Add((byte)value[i]);
                        if (value[i + 1] == '\n')
                        {
                            i++;
                        }
                        break;
                    default:
                        temp.Add((byte)value[i]);
                        break;
                }
            }

            return temp.ToArray();
        }

        public byte[] ToByteArray()
        {
            return bytes;
        }

        /// <summary>
        /// returns an int stored in this string
        /// e.g. if the string is <00FF> then 255 will be returned
        /// </summary>
        public int ToBigEndianInt()
        {
            int code = 0;

            for (int i = 0; i < bytes.Length; ++i)
            {
                code <<= 8;
                code |= bytes[i] & 0xFF;
            }

            return code;
        }

        public static bool operator ==(PdfString a, PdfString b)
        {
            if (ReferenceEquals(a, null) && ReferenceEquals(b, null))
            {
                return true;
            }

            if (ReferenceEquals(a, null) || ReferenceEquals(b, null))
            {
                return false;
            }

            return a.value == b.value;
        }

        public static bool operator !=(PdfString a, PdfString b)
        {
            return a.value != b.value;
        }

        public override string ToString()
        {
            return value;
        }

        public string ToString(Encoding encoding)
        {
            return encoding.GetString(bytes);
        }

        public override int GetHashCode()
        {
            return value.GetHashCode();
        }

        public override bool Equals(object obj)
        {
            if (obj is PdfString)
            {
                return ((PdfString)obj).value.Equals(value);
            }
            else if (obj is string)
            {
                return ((string)obj).Equals(value);
            }
            else
            {
                return base.Equals(obj);
            }
        }
    }
}
