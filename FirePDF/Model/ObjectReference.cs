using FirePDF.Reading;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FirePDF.Model
{
    public class ObjectReference
    {
        private PDF pdf;
        public readonly int objectNumber;
        public readonly int generation;

        public ObjectReference(PDF pdf, int objectNumber, int generation)
        {
            this.pdf = pdf;
            this.objectNumber = objectNumber;
            this.generation = generation;
        }

        public T get<T>()
        {
            return (T)pdf.get<T>(this);
        }

        public override string ToString()
        {
            return $"{objectNumber} {generation} R";
        }
    }
}
