using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;

namespace FirePDF.Model
{
    public class Operation
    {
        public string operatorName;
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
                return string.Join(" ", operands.Select(OperandToString)) + " " + operatorName;
            }
            else
            {
                return operatorName;
            }
        }

        public int GetOperandAsInt(int index)
        {
            return Convert.ToInt32(operands[index]);
        }

        public float GetOperandAsFloat(int index)
        {
            return (float)Convert.ToDouble(operands[index]);
        }
        
        public Name GetOperandAsName(int index)
        {
            return (Name)operands[index];
        }

        private static string OperandToString(object operand)
        {
            switch (operand)
            {
                case byte[] bytes:
                    return BitConverter.ToString(bytes).Replace("-", "");
                case IEnumerable<object> objects:
                    return "[" + string.Join(" ", objects).Select(x => OperandToString(x)) + "]";
                case Name name:
                    return string.Join("", ((string)name).Select(x => x < 32 || x > 128 ? @"\u" + (int)x : x.ToString()));
                default:
                    return Convert.ToString(operand);
            }
        }

        public PointF[] GetOperandsAsPointFs()
        {
            PointF[] points = new PointF[operands.Count / 2];

            for (int i = 0; i < points.Length; i++)
            {
                int j = i * 2;
                points[i] = new PointF((float)Convert.ToDouble(operands[j]), (float)Convert.ToDouble(operands[j + 1]));
            }

            return points;
        }

        public List<int> GetOperandsAsInts()
        {
            return operands.Select(Convert.ToInt32).ToList();
        }

        public List<float> GetOperandsAsFloats()
        {
            return operands.Select(x => (float)Convert.ToDouble(x)).ToList();
        }

        public List<double> GetOperandsAsDoubles()
        {
            return operands.Select(Convert.ToDouble).ToList();
        }
    }
}
