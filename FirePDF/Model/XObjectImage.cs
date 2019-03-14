using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FirePDF.Model
{
    class XObjectImage
    {
        private PDF pdf;
        private Dictionary<string, object> underlyingDict;

        public XObjectImage(PDF pdf)
        {
            this.pdf = pdf;
        }

        public void fromDictionary(Dictionary<string, object> dict)
        {
            underlyingDict = dict;
        }

        internal Image getImage()
        {
            return new Bitmap(100, 100);
        }
    }
}
