namespace FirePDF.Text
{
    public class CidRange
    {
        private readonly int from;
        private int to;
        private readonly int cid;

        public CidRange(int from, int to, int cid)
        {
            this.from = from;
            this.to = to;
            this.cid = cid;
        }

        
        public int CodeToCid(int code)
        {
            if (from <= code && code <= to)
            {
                return cid + (code - from);
            }
            else
            {
                return -1;
            }
        }

        /// <summary>
        /// if two CIDRanges are next to each other then they can be represented by a single range
        /// this method tries to extend the current range to include the new range, and returns true if it can
        /// </summary>
        public bool TryExtend(int newFrom, int newTo, int newCid)
        {
            if(to + 1 != newFrom || cid + (to - @from) + 1 != newCid)
            {
                return false;
            }

            to = newTo;
            return true;
        }
    }
}
