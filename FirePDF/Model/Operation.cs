using System;
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


        public PointF[] getOperationsAsPointFs()
        {
            PointF[] points = new PointF[operands.Count / 2];

            for (int i = 0; i < points.Length; i++)
            {
                int j = i * 2;
                points[i] = new PointF((float)Convert.ToDouble(operands[i]), (float)Convert.ToDouble(operands[i + 1]));
            }

            return points;
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
