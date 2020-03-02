using System;
using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FirePDF.Model
{
    public class PDFList : IHavePDF, IHaveChildren, IEnumerable<object>
    {
        private List<object> inner;
        private bool _isDirty = false;

        public PDFList(PDF pdf) : base(pdf)
        {
            inner = new List<object>();
        }

        public PDFList(PDF pdf, List<object> inner) : base(pdf)
        {
            this.inner = inner;
        }

        public PDFList(PDF pdf, params object[] values) : base(pdf)
        {
            inner = new List<object>(values);
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
        
        public bool isDirty() => _isDirty || inner.Where(x => x is IHaveChildren).Any(x => ((IHaveChildren)x).isDirty());
        
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
            if (index < inner.Count)
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

        public void set(int index, ObjectReference objectReference)
        {
            inner[index] = objectReference;
            _isDirty = true;
        }

        public void insert(int i, ObjectReference objRef)
        {
            inner.Insert(i, objRef);
            _isDirty = true;
        }

        public void add(ObjectReference objRef)
        {
            inner.Add(objRef);
            _isDirty = true;
        }

        public IEnumerator<object> GetEnumerator() => inner.GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => inner.GetEnumerator();

        public IEnumerable<ObjectReference> GetObjectReferences()
        {
            foreach (object value in inner)
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

        public void swapReferences(Func<ObjectReference, ObjectReference> callback)
        {
            List<object> newInner = new List<object>();
            foreach(object obj in inner)
            {
                if(obj is ObjectReference)
                {
                    ObjectReference newReference = callback(obj as ObjectReference);
                    if(obj != newReference)
                    {
                        newInner.Add(newReference);
                        _isDirty = true;
                    }
                    else
                    {
                        newInner.Add(obj);
                    }
                }
                else
                {
                    newInner.Add(obj);
                    if (obj is IHaveChildren)
                    {
                        (obj as IHaveChildren).swapReferences(callback);
                    }
                }
            }

            inner = newInner;
        }
    }
}
