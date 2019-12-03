using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FirePDF.Writing
{
    public static class ByteWriter
    {
        public static string byteArrayToHexString(byte[] buffer)
        {
            string hex = BitConverter.ToString(buffer);
            return hex.Replace("-", "");
        }
    }
}
