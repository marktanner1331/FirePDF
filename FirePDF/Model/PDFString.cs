using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace FirePDF.Model
{
    public class PDFString
    {
        private readonly byte[] bytes;
        private readonly string value;
        public readonly bool isHexString;

        /// <summary>
        /// returns the number of bytes used to store this string
        /// for example, the length of <00FF> is 2
        /// </summary>
        public int length => bytes.Length;

        public PDFString(byte[] bytes)
        {
            this.bytes = bytes;
            this.isHexString = true;
            value = Encoding.ASCII.GetString(bytes);
        }

        public PDFString(byte[] bytes, bool isHexString)
        {
            this.bytes = bytes;
            this.isHexString = isHexString;
            value = Encoding.ASCII.GetString(bytes);
        }

        public PDFString(string value)
        {
            this.value = value;
            this.isHexString = false;
            bytes = Encoding.ASCII.GetBytes(value);
        }

        public byte[] toByteArray()
        {
            return bytes;
        }

        /// <summary>
        /// returns an int stored in this string
        /// e.g. if the string is <00FF> then 255 will be returned
        /// </summary>
        public int toBigEndianInt()
        {
            int code = 0;

            for (int i = 0; i < bytes.Length; ++i)
            {
                code <<= 8;
                code |= (bytes[i] & 0xFF);
            }

            return code;
        }

        public static bool operator ==(PDFString a, PDFString b)
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

        public static bool operator !=(PDFString a, PDFString b)
        {
            return a.value != b.value;
        }

        public override string ToString()
        {
            return value;
        }

        public string toString(Encoding encoding)
        {
            return encoding.GetString(bytes);
        }

        public override int GetHashCode()
        {
            return value.GetHashCode();
        }

        public override bool Equals(object obj)
        {
            if (obj is PDFString)
            {
                return ((PDFString)obj).value.Equals(value);
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
