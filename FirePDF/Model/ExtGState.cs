namespace FirePDF.Model
{
    public class ExtGState : HaveUnderlyingDict
    {
        public ExtGState(PdfDictionary underlyingDict) : base(underlyingDict)
        {
        }

        /// <summary>
        /// returns true of this ExtGState sets overprinting to true for either stroking or non stroking operations
        /// </summary>
        public bool enablesOverprinting()
        {
            if (UnderlyingDict.ContainsKey("OP"))
            {
                return true;
            }

            if (UnderlyingDict.ContainsKey("op"))
            {
                return true;
            }

            if (UnderlyingDict.ContainsKey("OPM"))
            {
                return true;
            }

            return false;
        }
    }
}