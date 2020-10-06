using System;
using System.Collections.Generic;
using System.Text;

namespace FirePDF.Model
{
    public class PatternDictionary : HaveUnderlyingDict
    {
        public PatternDictionary(PdfDictionary underlyingDict) : base(underlyingDict)
        {
        }
    }
}
