using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace FirePDF.Model
{
    public class PDFString
    {
        public readonly byte[] bytes;
        private readonly string value;
        public readonly bool isHexString;

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

        public static implicit operator string(PDFString value)
        {
            return value.value;
        }

        public static implicit operator PDFString(string value)
        {
            return new PDFString(value);
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
                return ((PDFString)(string)obj).value.Equals(value);
            }
            else
            {
                return base.Equals(obj);
            }
        }
    }
}
