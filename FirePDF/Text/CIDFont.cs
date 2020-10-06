using FirePDF.Model;

namespace FirePDF.Text
{
    public abstract class CidFont : Font
    {
        protected CidFont(PdfDictionary dictionary) : base(dictionary)
        {

        }

        /// <summary>
        /// returns the width of the given cid in user space
        /// </summary>
        public abstract float GetWidthForCid(int cid);
    }
}