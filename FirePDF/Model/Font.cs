using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FirePDF.Model
{
    abstract class Font
    {
        private PDF pdf;
        private Dictionary<Name, object> dictionary;

        protected Font(PDF pdf, Dictionary<Name, object> dictionary)
        {
            this.pdf = pdf;
            this.dictionary = dictionary;
        }

        public static Font loadExistingFontFromPDF(PDF pdf, Dictionary<Name, object> dictionary)
        {
            Name subType = dictionary.ContainsKey("Subtype") ? (Name)dictionary["Subtype"] : null;
            switch(subType.ToString())
            {
                case "Type0":
                    return new Type0Font(pdf, dictionary);
                default:
                    throw new NotImplementedException();
            }
        }

        public void getToUnicodeMap()
        {
            if(dictionary.ContainsKey("ToUnicode") == false)
            {
                return;
            }


        }
    }
}
