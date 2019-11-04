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
        private PDFDictionary dictionary;

        protected Font(PDF pdf, PDFDictionary dictionary)
        {
            this.pdf = pdf;
            this.dictionary = dictionary;
        }

        public static Font loadExistingFontFromPDF(PDF pdf, PDFDictionary dictionary)
        {
            Name subType = dictionary.get<Name>("Subtype");
            switch(subType)
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
