using System;
using System.Collections.Generic;
using System.Text;

namespace FirePDF.Model
{
    public class SoftMask : HaveUnderlyingDict
    {
        public SoftMask(PdfDictionary underlyingDict) : base(underlyingDict)
        {

        }

        public XObjectForm TransparencyGroup => UnderlyingDict.Get<XObjectForm>("G");

        public Name SubType => UnderlyingDict.Get<Name>("S");
    }
}
