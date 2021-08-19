using System;
using System.Collections.Generic;
using System.Text;

namespace FirePDF.Model
{
    public class TransparencyGroup : HaveUnderlyingDict
    {
        public TransparencyGroup(PdfDictionary dictionary) : base(dictionary)
        {

        }

        public bool Isolated
        {
            get => UnderlyingDict.ContainsKey("I") && UnderlyingDict.Get<bool>("I");
            set => UnderlyingDict.Set("I", value);
        }

        public bool Knockout
        {
            get => UnderlyingDict.ContainsKey("K") && UnderlyingDict.Get<bool>("K");
            set => UnderlyingDict.Set("K", value);
        }

    }
}
