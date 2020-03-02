﻿using FirePDF.Reading;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FirePDF.Model
{
    public class ObjectReference : IHavePDF
    {
        public readonly int objectNumber;
        public readonly int generation;
        public int hash => GetHashCode();
        
        public ObjectReference(PDF pdf, int objectNumber, int generation) : base(pdf)
        {
            this.objectNumber = objectNumber;
            this.generation = generation;
        }

        public override int GetHashCode()
        {
            return (int)((objectNumber & 0xffffffff) << 16) + (generation & 0xffff);
        }

        public override bool Equals(object obj)
        {
            if(obj is ObjectReference)
            {
                ObjectReference objRef = obj as ObjectReference;

                return pdf == objRef.pdf
                    && objectNumber == objRef.objectNumber
                    && generation == objRef.generation;
            }
            else
            {
                return base.Equals(obj);
            }
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
