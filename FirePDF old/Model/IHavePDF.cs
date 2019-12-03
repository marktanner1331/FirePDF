using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FirePDF.Model
{
    public abstract class IHavePDF
    {
        public PDF pdf { get; protected set; }
        public IHavePDF(PDF pdf)
        {
            this.pdf = pdf;
        }
    }
}
