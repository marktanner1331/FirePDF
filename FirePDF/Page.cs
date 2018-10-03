using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FirePDF
{
    public class Page
    {
        private PDF pdf;

        public Page(PDF pdf)
        {
            this.pdf = pdf;
        }

        public void fromDictionary(Dictionary<string, object> dict)
        {

        }
    }
}
