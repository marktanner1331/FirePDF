using FirePDF.Writing;

namespace FirePDF.Model
{
    public class Trailer : HaveUnderlyingDict
    {
        public int? Size
        {
            get => UnderlyingDict.Get<int?>("Size");
            set => UnderlyingDict.Set("Size", value);
        }

        public ObjectReference Root
        {
            get => UnderlyingDict.Get<ObjectReference>("Root");
            set => UnderlyingDict.Set("Root", value);
        }

        public int? Prev
        {
            get => UnderlyingDict.Get<int?>("Prev");
            set => UnderlyingDict.Set("Prev", value);
        }
        
        public int? XRefStm
        {
            get => UnderlyingDict.Get<int?>("XRefStm");
            set => UnderlyingDict.Set("XRefStm", value);
        }

        public Trailer(Pdf pdf) : base(new PdfDictionary(pdf))
        {

        }

        public Trailer(PdfDictionary underlyingDict) : base(underlyingDict)
        {
            
        }

        public void Serialize(PdfWriter writer)
        {
            writer.WriteAscii("trailer");
            writer.WriteNewLine();
            
            writer.WriteDirectObject(UnderlyingDict);
            writer.WriteNewLine();
        }

    }
}