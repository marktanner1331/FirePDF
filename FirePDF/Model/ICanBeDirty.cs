using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FirePDF.Model
{
    internal interface ICanBeDirty
    {
        //will check subdictionaries and sublists but will not resolve object references
        bool isDirtyShallow();

        //will resolve object references
        bool isDirtyRecursive();
    }
}
