using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FirePDF.Model
{
    public class PDFList
    {
        private List<object> inner;
        private readonly PDF pdf;

        public PDFList(PDF pdf)
        {
            this.pdf = pdf;
            inner = new List<object>();
        }

        public RectangleF asRectangle()
        {
            float left = (float)Convert.ToDouble(inner[0]);
            float bottom = (float)Convert.ToDouble(inner[1]);
            float right = (float)Convert.ToDouble(inner[2]);
            float top = (float)Convert.ToDouble(inner[3]);

            return new RectangleF
            {
                X = left,
                Y = bottom,
                Width = right - left,
                Height = top - bottom
            };
        }

        public void add(object value) => inner.Add(value);
        public int count => inner.Count;

        public List<T> cast<T>(bool resolveReferences = true)
        {
            List<T> temp = new List<T>();

            foreach(object value in inner)
            {
                //if we get an object reference, and we don't want an object reference, then resolve it
                if (value is ObjectReference && resolveReferences && typeof(T) != typeof(ObjectReference))
                {
                    temp.Add((value as ObjectReference).get<T>());
                }
                else
                {
                    temp.Add((T)value);
                }
            }

            return temp;
        }

        public T get<T>(int index, bool resolveReferences = true)
        {
            if (index >= inner.Count)
            {
                object value = inner[index];

                //if we get an object reference, and we don't want an object reference, then resolve it
                if (value is ObjectReference && resolveReferences && typeof(T) != typeof(ObjectReference))
                {
                    return (value as ObjectReference).get<T>();
                }
                else
                {
                    return (T)value;
                }
            }
            else
            {
                throw new IndexOutOfRangeException();
            }
        }

        public void set(int index, ObjectReference objectReference) => inner[index] = objectReference;
    }
}
