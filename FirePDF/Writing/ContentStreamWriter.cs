using FirePDF.Model;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FirePDF.Writing
{
    public static class ContentStreamWriter
    {
        public static void writeOperationsToStream(Stream stream, IEnumerable<Operation> operations)
        {
            using (PDFWriter writer = new PDFWriter(stream, true))
            {
                foreach (Operation operation in operations)
                {
                    foreach (object operand in operation.operands)
                    {
                        writer.writeDirectObject(operand);
                        writer.writeASCII(" ");
                    }

                    writer.writeASCII(operation.operatorName);
                    writer.writeNewLine();
                }
            }
        }
    }
}
