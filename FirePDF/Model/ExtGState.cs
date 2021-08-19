using System;

namespace FirePDF.Model
{
    public class ExtGState : HaveUnderlyingDict
    {
        public ExtGState(PdfDictionary underlyingDict) : base(underlyingDict)
        {
        }

        public bool HasSoftMask()
        {
            if (UnderlyingDict.ContainsKey("SMask") == false)
            {
                return false;
            }

            object temp = UnderlyingDict.Get("SMask", true);
            if (temp is Name && (Name)temp == "None")
            {
                return false;
            }

            return true;
        }

        public bool HasSoftMask(out SoftMask softMask)
        {
            if(UnderlyingDict.ContainsKey("SMask") == false)
            {
                softMask = null;
                return false;
            }

            object temp = UnderlyingDict.Get("SMask", true);
            if(temp is Name && (Name)temp == "None")
            {
                softMask = null;
                return false;
            }

            if(temp is SoftMask)
            {
                softMask = (SoftMask)temp;
            }
            else if(temp is PdfDictionary)
            {
                softMask = new SoftMask(temp as PdfDictionary);
            }
            else
            {
                throw new Exception();
            }
            
            return true;
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