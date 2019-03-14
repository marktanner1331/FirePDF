using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FirePDF.Model
{
    public class Rectangle
    {
        public readonly float left = 0;
        public readonly float bottom = 0;
        public readonly float right = 0;
        public readonly float top = 0;

        public Rectangle(float left, float bottom, float right, float top)
        {
            this.left = left;
            this.bottom = bottom;
            this.right = right;
            this.top = top;
        }

        public Rectangle(List<object> array)
        {
            left = objectToFloat(array[0]);
            bottom = objectToFloat(array[1]);
            right = objectToFloat(array[2]);
            top = objectToFloat(array[3]);
        }

        private float objectToFloat(object value)
        {
            return value is int
                ? (int)value
                : (float)value;
        }
    }
}
