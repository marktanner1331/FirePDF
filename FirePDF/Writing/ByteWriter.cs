using System;

namespace FirePDF.Writing
{
    public static class ByteWriter
    {
        public static string ByteArrayToHexString(byte[] buffer)
        {
            string hex = BitConverter.ToString(buffer);
            return hex.Replace("-", "");
        }
    }
}
