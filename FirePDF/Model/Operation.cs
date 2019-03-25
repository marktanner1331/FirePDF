﻿using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FirePDF.Model
{
    public class Operation
    {
        public string operatorName;

        //TODO keep this nullable for operators without operands -> improves performance
        public List<object> operands;

        public Operation()
        {
            operands = new List<object>();
        }

        public Operation(string operatorName, List<object> operands)
        {
            this.operatorName = operatorName;
            this.operands = operands;
        }

        public override string ToString()
        {
            if(operands.Count > 0)
            {
                return string.Join(" ", operands.Select(x => operandToString(x))) + " " + operatorName;
            }
            else
            {
                return operatorName;
            }
        }

        public float getOperandAsFloat(int index)
        {
            return (float)Convert.ToDouble(operands[index]);
        }

        private string operandToString(object operand)
        {
            if(operand is byte[])
            {
                return BitConverter.ToString((byte[])operand).Replace("-", "");
            }
            else if(operand is IEnumerable<object>)
            {
                return "[" + string.Join(" ", (IEnumerable<object>)operand).Select(x => operandToString(x)) + "]";
            }
            else if(operand is Name)
            {
                return string.Join("", ((string)(Name)operand).Select(x => x < 32 || x > 128 ? @"\u" + (int)x : x.ToString()));
            }
            else
            {
                return Convert.ToString(operand);
            }
        }

        public PointF[] getOperationsAsPointFs()
        {
            PointF[] points = new PointF[operands.Count / 2];

            for (int i = 0; i < points.Length; i++)
            {
                int j = i * 2;
                points[i] = new PointF((float)Convert.ToDouble(operands[j]), (float)Convert.ToDouble(operands[j + 1]));
            }

            return points;
        }

        public List<int> getOperationsAsInts()
        {
            //TODO rename this to "operand"
            return operands.Select(x => Convert.ToInt32(x)).ToList();
        }

        public List<float> getOperationsAsFloats()
        {
            return operands.Select(x => (float)Convert.ToDouble(x)).ToList();
        }

        public List<double> getOperationsAsDoubles()
        {
            return operands.Select(x => Convert.ToDouble(x)).ToList();
        }
    }
}
