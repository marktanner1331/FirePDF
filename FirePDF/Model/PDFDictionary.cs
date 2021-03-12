using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace FirePDF.Model
{
    //TODO: merge some of these interfaces
    public class PdfDictionary : HavePdf, IHaveChildren, IEnumerable<KeyValuePair<Name, object>>
    {
        private readonly Dictionary<Name, object> inner;
        private bool isDirty = false;

        public PdfDictionary(Pdf pdf, Dictionary<Name, object> inner) : base(pdf)
        {
            this.inner = inner;
        }

        public PdfDictionary(Pdf pdf) : base(pdf)
        {
            inner = new Dictionary<Name, object>();
        }

        public bool ContainsKey(Name key) => inner.ContainsKey(key);

        public IEnumerable<Name> Keys => inner.Keys;
        
        public void Set(Name key, object value)
        {
            inner[key] = value;
            isDirty = true;
        }
        
        internal void SetWithoutDirtying(Name key, object value)
        {
            inner[key] = value;
        }

        public void RemoveEntry(Name key)
        {
            inner.Remove(key);
            isDirty = true;
        }

        public object Get(Name key, bool resolveReferences)
        {
            if (inner.ContainsKey(key))
            {
                object value = inner[key];

                //if we get an object reference, and we don't want an object reference, then resolve it
                if (value is ObjectReference reference && resolveReferences)
                {
                    return Pdf.store.Get<object>(reference);
                }

                return value;
            }
            else
            {
                return null;
            }
        }

        public T Get<T>(Name key)
        {
            if (inner.ContainsKey(key))
            {
                object value = inner[key];

                //if we get an object reference, and we don't want an object reference, then resolve it
                if(value is ObjectReference reference && typeof(T) != typeof(ObjectReference))
                {
                    return Pdf.store.Get<T>(reference);
                }

                if(value is long && typeof(T) == typeof(int))
                {
                    value = (int)(long)value;
                }

                return (T)value;
            }
            else
            {
                return default;
            }
        }

        public IEnumerator<KeyValuePair<Name, object>> GetEnumerator() => inner.GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => inner.GetEnumerator();

        public IEnumerable<ObjectReference> GetObjectReferences()
        {
            foreach(object value in inner.Values)
            {
                if (value is ObjectReference)
                {
                    yield return value as ObjectReference;
                }
                else if (value is IHaveChildren)
                {
                    foreach (ObjectReference subValue in (value as IHaveChildren).GetObjectReferences())
                    {
                        yield return subValue;
                    }
                }
            }
        }

        public bool IsDirty() => isDirty || inner.Values.Where(x => x is IHaveChildren).Any(x => ((IHaveChildren)x).IsDirty());
        
        public void SwapReferences(Func<ObjectReference, ObjectReference> callback)
        {
            foreach(Name key in inner.Keys.ToList())
            {
                switch (inner[key])
                {
                    case ObjectReference reference:
                    {
                        ObjectReference newReference = callback(reference);
                        if(!reference.Equals(newReference))
                        {
                            inner[key] = newReference;
                            isDirty = true;
                        }
                        else
                        {
                                object obj = reference.Get<object>();
                                if(obj is IHaveChildren children)
                                {
                                    children.SwapReferences(callback);
                                }
                        }

                        break;
                    }
                    case IHaveChildren children:
                        children.SwapReferences(callback);
                        break;
                }
            }
        }
    }
}
