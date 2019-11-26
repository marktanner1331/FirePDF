namespace FirePDF.Model
{
    public abstract class CIDFont : Font
    {
        public CIDFont(PDFDictionary dictionary) : base(dictionary)
        {

        }

        /// <summary>
        /// returns the width of the given cid in user space
        /// </summary>
        public abstract float getWidthForCID(int cid);
    }
}