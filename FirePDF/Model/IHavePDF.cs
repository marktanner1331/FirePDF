namespace FirePDF.Model
{
    public abstract class HavePdf
    {
        public Pdf Pdf { get; protected set; }

        protected HavePdf(Pdf pdf)
        {
            Pdf = pdf;
        }
    }
}
