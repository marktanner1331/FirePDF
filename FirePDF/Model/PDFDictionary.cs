using FirePDF.Reading;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FirePDF.Model
{
    public class PDFDictionary
    {
        private Dictionary<Name, object> inner;
        private readonly PDF pdf;

        public PDFDictionary(PDF pdf)
        {
            this.pdf = pdf;
            inner = new Dictionary<Name, object>();
        }

        public bool ContainsKey(string key) => inner.ContainsKey(key);

        public IEnumerable<Name> keys => inner.Keys;

        public void set(Name key, object value)
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
                    value = PDFReader.readIndirectObject(pdf, value as ObjectReference);
                }

                return (T)value;
            }
            else
            {
                throw new KeyNotFoundException();
            }
        }
    }
}
