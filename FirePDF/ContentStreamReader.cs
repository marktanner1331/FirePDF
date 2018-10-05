using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FirePDF
{
    public static class ContentStreamReader
    {
        public static List<Operation> readContentStream(Stream decompressedStream)
        {
            List<Operation> operations = new List<Operation>();
            Operation currentOperation = new Operation();

            readTokens(
                decompressedStream,
                operatorName =>
                {
                    currentOperation.operatorName = operatorName;
                    operations.Add(currentOperation);
                    currentOperation = new Operation();
                },
                operand => currentOperation.operands.Add(operand));

            return operations;
        }

        private static void readTokens(Stream stream, Action<string> foundOperator, Action<object> foundOperand)
        {
            char current = (char)stream.ReadByte();
            if(current == )
        }
    }
}
