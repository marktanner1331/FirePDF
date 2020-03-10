namespace FirePDF.Model
{
    public class ObjectReference : HavePdf
    {
        public readonly int objectNumber;
        public readonly int generation;
        public int Hash => GetHashCode();
        
        public ObjectReference(Pdf pdf, int objectNumber, int generation) : base(pdf)
        {
            this.objectNumber = objectNumber;
            this.generation = generation;
        }

        public override int GetHashCode()
        {
            return (int)((objectNumber & 0xffffffff) << 16) + (generation & 0xffff);
        }

        public override bool Equals(object obj)
        {
            if(obj is ObjectReference objRef)
            {
                return Pdf == objRef.Pdf
                    && objectNumber == objRef.objectNumber
                    && generation == objRef.generation;
            }
            else
            {
                return base.Equals(obj);
            }
        }

        public T Get<T>()
        {
            return (T)Pdf.Get<T>(this);
        }

        public override string ToString()
        {
            return $"{objectNumber} {generation} R";
        }
    }
}
