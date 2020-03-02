using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FirePDF.Reading
{
    public class ByteReader
    {
        public static int readBigEndianInt(byte[] buffer)
        {
            return readBigEndianInt(buffer, buffer.Length);
        }

        /// <summary>
        /// reads a big endian int from the start of the buffer, consuming 'length' bytes
        /// </summary>
        public static int readBigEndianInt(byte[] buffer, int length)
        {
            int code = 0;

            for (int i = 0; i < length; ++i)
            {
                code <<= 8;
                code |= (buffer[i] & 0xFF);
            }

            return code;
        }
    }
}
