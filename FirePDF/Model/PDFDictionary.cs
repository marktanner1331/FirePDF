using FirePDF.Reading;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FirePDF.Model
{
    //TODO: merge some of these interfaces
    public class PDFDictionary : IHavePDF, IHaveChildren, IEnumerable<KeyValuePair<Name, object>>
    {
        private Dictionary<Name, object> inner;
        private bool _isDirty = false;

        public PDFDictionary(PDF pdf, Dictionary<Name, object> inner) : base(pdf)
        {
            this.inner = inner;
        }

        public PDFDictionary(PDF pdf) : base(pdf)
        {
            this.inner = new Dictionary<Name, object>();
        }

        public bool containsKey(Name key) => inner.ContainsKey(key);

        public IEnumerable<Name> keys => inner.Keys;
        
        public void set(Name key, object value)
        {
            inner[key] = value;
            _isDirty = true;
        }
        
        internal void setWithoutDirtying(Name key, object value)
        {
            inner[key] = value;
        }

        public T get<T>(Name key, bool resolveReferences = true)
        {
            if (inner.ContainsKey(key))
            {
                object value = inner[key];

                //if we get an object reference, and we don't want an object reference, then resolve it
                if(value is ObjectReference && resolveReferences && typeof(T) != typeof(ObjectReference))
                {
                    return pdf.store.get<T>(value as ObjectReference);
                }

                return (T)value;
            }
            else
            {
                return default(T);
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

        public bool isDirty() => _isDirty || inner.Values.Where(x => x is IHaveChildren).Any(x => ((IHaveChildren)x).isDirty());
        
        public void swapReferences(Func<ObjectReference, ObjectReference> callback)
        {
            foreach(Name key in inner.Keys.ToList())
            {
                if(inner[key] is ObjectReference)
                {
                    ObjectReference newReference = callback((ObjectReference)inner[key]);
                    if(inner[key] != newReference)
                    {
                        inner[key] = newReference;
                        _isDirty = true;
                    }
                }
                else if(inner[key] is IHaveChildren)
                {
                    (inner[key] as IHaveChildren).swapReferences(callback);
                }
            }
        }
    }
}
