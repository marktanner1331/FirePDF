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

        public PdfString(string value)
        {
            this.value = value;
            isHexString = false;
            bytes = Encoding.ASCII.GetBytes(value);
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
