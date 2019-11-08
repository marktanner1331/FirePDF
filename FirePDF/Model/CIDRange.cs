using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FirePDF.Model
{
    public class CIDRange
    {
        private readonly int from;
        private int to;
        private readonly int cid;

        public CIDRange(int from, int to, int cid)
        {
            this.from = from;
            this.to = to;
            this.cid = cid;
        }

        
        public int codeToCID(int code)
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
        /// if two CIDRanges are next to eachother then they can be represented by a single range
        /// this method tries to extend the current range to include the new range, and returns true if it can
        /// </summary>
        public bool tryExtend(int newFrom, int newTo, int newCID)
        {
            if(to + 1 != newFrom || cid + (to - from + 1) != newCID)
            {
                return false;
            }

            to = newTo;
            return true;
        }
    }
}
