using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FirePDF
{
    public static class FileReader
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
    }
}
