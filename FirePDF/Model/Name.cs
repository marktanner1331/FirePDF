using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FirePDF.Model
{
    public class Name
    {
        private string value;

        public Name(string value)
        {
            this.value = value;
        }

        public static implicit operator string(Name name)
        {
            return name.value;
        }

        public static implicit operator Name(string value)
        {
            return new Name(value);
        }
        
        public static bool operator ==(Name a, Name b)
        {
            return a.value == b.value;
        }

        public static bool operator !=(Name a, Name b)
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
            if(obj is Name)
            {
                return ((Name)obj).value.Equals(value);
            }
            else
            {
                return base.Equals(obj);
            }
        }
    }
}
