using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FirePDF.Model
{
    /// <summary>
    /// A codespacerange defines the range of a codespace
    /// a range of '<00> <80>' means that the space consists of 1 byte codes which start at 00 and end at 80
    /// </summary>
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
