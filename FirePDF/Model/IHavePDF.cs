using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FirePDF.Model
{
    public abstract class IHavePDF
    {
        public readonly PDF pdf;
        public IHavePDF(PDF pdf)
        {
            this.pdf = pdf;
        }
    }
}
