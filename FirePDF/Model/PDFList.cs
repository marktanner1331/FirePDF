using System;
using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;

namespace FirePDF.Model
{
    public class PdfList : HavePdf, IHaveChildren, IEnumerable<object>
    {
        private List<object> inner;
        private bool isDirty = false;

        public PdfList(Pdf pdf) : base(pdf)
        {
            inner = new List<object>();
        }

        public PdfList(Pdf pdf, List<object> inner) : base(pdf)
        {
            this.inner = inner;
        }

        public PdfList(Pdf pdf, params object[] values) : base(pdf)
        {
            inner = new List<object>(values);
        }

        public RectangleF AsRectangle()
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

        public void Add(object value) => inner.Add(value);
        public int Count => inner.Count;
        
        public bool IsDirty() => isDirty || inner.Where(x => x is IHaveChildren).Any(x => ((IHaveChildren)x).IsDirty());
        
        public List<T> Cast<T>(bool resolveReferences = true)
        {
            List<T> temp = new List<T>();

            foreach(object value in inner)
            {
                //if we get an object reference, and we don't want an object reference, then resolve it
                if (value is ObjectReference && resolveReferences && typeof(T) != typeof(ObjectReference))
                {
                    temp.Add((value as ObjectReference).Get<T>());
                }
                else
                {
                    temp.Add((T)value);
                }
            }

            return temp;
        }

        public T Get<T>(int index, bool resolveReferences = true)
        {
            if (index < inner.Count)
            {
                object value = inner[index];

                //if we get an object reference, and we don't want an object reference, then resolve it
                if (value is ObjectReference && resolveReferences && typeof(T) != typeof(ObjectReference))
                {
                    return (value as ObjectReference).Get<T>();
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

        public void Set(int index, ObjectReference objectReference)
        {
            inner[index] = objectReference;
            isDirty = true;
        }

        public void Insert(int i, ObjectReference objRef)
        {
            inner.Insert(i, objRef);
            isDirty = true;
        }

        public void Add(ObjectReference objRef)
        {
            inner.Add(objRef);
            isDirty = true;
        }

        public IEnumerator<object> GetEnumerator() => inner.GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => inner.GetEnumerator();

        public IEnumerable<ObjectReference> GetObjectReferences()
        {
            foreach (object value in inner)
            {
                switch (value)
                {
                    case ObjectReference reference:
                        yield return reference;
                        break;
                    case IHaveChildren children:
                    {
                        foreach (ObjectReference subValue in children.GetObjectReferences())
                        {
                            yield return subValue;
                        }

                        break;
                    }
                }
            }
        }

        public void SwapReferences(Func<ObjectReference, ObjectReference> callback)
        {
            List<object> newInner = new List<object>();
            foreach(object obj in inner)
            {
                if(obj is ObjectReference)
                {
                    ObjectReference newReference = callback(obj as ObjectReference);
                    if(!obj.Equals(newReference))
                    {
                        newInner.Add(newReference);
                        isDirty = true;
                    }
                    else
                    {
                        newInner.Add(obj);
                    }
                }
                else
                {
                    newInner.Add(obj);
                    if (obj is IHaveChildren children)
                    {
                        children.SwapReferences(callback);
                    }
                }
            }

            inner = newInner;
        }
    }
}
