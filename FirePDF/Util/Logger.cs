using FirePDF.Model;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FirePDF.Util
{
    public static class Logger
    {
        public static void logOperationsWithLineNumbers(IEnumerable<Operation> operations)
        {
            int i = 0;
            foreach(Operation operation in operations)
            {
                Debug.WriteLine(i + ") " + operation.ToString());
                i++;
            }
        }

        public static void logOperations(List<Operation> operations)
        {
            foreach (Operation operation in operations)
            {
                Debug.WriteLine(operation.ToString());
            }
        }
    }
}
