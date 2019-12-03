using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FirePDF.Model
{
    public class CodeSpaceRange
    {
        public readonly int start;
        public readonly int end;
        public readonly int codeLength;

        public CodeSpaceRange(int start, int end, int codeLength)
        {
            this.start = start;
            this.end = end;
            this.codeLength = codeLength;
        }

        public bool isInRange(int code)
        {
            return code >= start && code <= end;
        }
    }
}
