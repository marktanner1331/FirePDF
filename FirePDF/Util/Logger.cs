using FirePDF.Model;
using System.Collections.Generic;
using System.Diagnostics;

namespace FirePDF.Util
{
    public static class Logger
    {
        public static void Warning(string message)
        {
            Debug.WriteLine(message);
        }

        public static void LogOperationsWithLineNumbers(IEnumerable<Operation> operations)
        {
            int i = 0;
            foreach(Operation operation in operations)
            {
                Debug.WriteLine(i + ") " + operation.ToString());
                i++;
            }
        }

        public static void LogOperations(List<Operation> operations)
        {
            foreach (Operation operation in operations)
            {
                Debug.WriteLine(operation.ToString());
            }
        }
    }
}
