using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FirePDF.Model
{
    interface IHaveChildren
    {
        IEnumerable<ObjectReference> GetObjectReferences();
        void swapReferences(Func<ObjectReference, ObjectReference> callback);

        //will check subdictionaries and sublists but will not resolve object references
        bool isDirty();
    }
}
