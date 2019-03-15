using System;
using System.Collections.Generic;
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

        internal List<double> getOperationsAsDoubles()
        {
            return operands.Select(x => Convert.ToDouble(x)).ToList();
        }
    }
}
