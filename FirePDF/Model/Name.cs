namespace FirePDF.Model
{
    public class Name
    {
        private readonly string value;

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
            if(ReferenceEquals(a, null) && ReferenceEquals(b, null))
            {
                return true;
            }

            if (ReferenceEquals(a, null) || ReferenceEquals(b, null))
            {
                return false;
            }
            
            return a.value == b.value;
        }

        public static bool operator !=(Name a, Name b)
        {
            return b is null == false && a.value != b.value;
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
            switch (obj)
            {
                case Name name:
                    return name.value.Equals(value);
                case string s:
                    return ((Name)s).value.Equals(value);
                default:
                    return base.Equals(obj);
            }
        }
    }
}
