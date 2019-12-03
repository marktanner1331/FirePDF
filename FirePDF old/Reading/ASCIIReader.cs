using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FirePDF.Reading
{
    public static class ASCIIReader
    {
        /// <summary>
        /// reads an ascii string from the stream at the given position
        /// </summary>
        public static string readASCIIString(Stream stream, int length)
        {
            byte[] buffer = new byte[length];
            stream.Read(buffer, 0, length);

            return Encoding.ASCII.GetString(buffer);
        }

        /// <summary>
        /// reads an integer stored as an ascii string
        /// </summary>
        public static int readASCIIInteger(Stream stream)
        {
            int i = 0;
            while (true)
            {
                byte current = (byte)stream.ReadByte();
                if(current < '0' || current > '9')
                {
                    stream.Position--;
                    return i;
                }

                i *= 10;
                i += current - '0';
            }
        }

        /// <summary>
        /// reads characters up until a new line character is found. the stream is advanced to the first byte of the next line
        /// </summary>
        public static string readLine(Stream stream)
        {
            StringBuilder sb = new StringBuilder();
            while(true)
            {
                byte current = (byte)stream.ReadByte();
                switch((char)current)
                {
                    case '\r':
                    case '\n':
                        goto end;
                    default:
                        sb.Append((char)current);
                        break;
                }
            }

            end:;

            while (true)
            {
                byte current = (byte)stream.ReadByte();
                switch ((char)current)
                {
                    case '\r':
                    case '\n':
                        break;
                    default:
                        stream.Position--;
                        return sb.ToString();
                }
            }
        }
    }
}
