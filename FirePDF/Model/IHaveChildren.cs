using System;
using System.Collections.Generic;

namespace FirePDF.Model
{
    public interface IHaveChildren
    {
        IEnumerable<ObjectReference> GetObjectReferences();
        void SwapReferences(Func<ObjectReference, ObjectReference> callback);

        //will check subdictionaries and sublists but will not resolve object references
        bool IsDirty();
    }
}
