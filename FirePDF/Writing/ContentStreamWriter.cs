using FirePDF.Model;
using System.Collections.Generic;
using System.IO;

namespace FirePDF.Writing
{
    public static class ContentStreamWriter
    {
        public static void WriteOperationsToStream(Stream stream, IEnumerable<Operation> operations)
        {
            using (PdfWriter writer = new PdfWriter(stream, true))
            {
                foreach (Operation operation in operations)
                {
                    foreach (object operand in operation.operands)
                    {
                        writer.WriteDirectObject(operand);
                        writer.WriteAscii(" ");
                    }

                    writer.WriteAscii(operation.operatorName);
                    writer.WriteNewLine();
                }
            }
        }
    }
}
