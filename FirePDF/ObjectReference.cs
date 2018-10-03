using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FirePDF
{
    public class ObjectReference
    {
        public int objectNumber;
        public int generation;

        public ObjectReference(int objectNumber, int generation)
        {
            this.objectNumber = objectNumber;
            this.generation = generation;
        }
    }
}
