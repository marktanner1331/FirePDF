using FirePDF.Model;
using System.Collections.Generic;
using System.IO;

namespace FirePDF.Writing
{
    public class ContentStreamWriter
    {
        private Stream stream;
        private PdfWriter writer;

        public ContentStreamWriter(Stream stream)
        {
            this.stream = stream;
            writer = new PdfWriter(stream, true);
        }

        public void writeOperation(Operation operation)
        {
            foreach (object operand in operation.operands)
            {
                writer.WriteDirectObject(operand);
                writer.WriteAscii(" ");
            }

            writer.WriteAscii(operation.operatorName);
            writer.WriteNewLine();
        }

        public void flush()
        {
            writer.Flush();
        }

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
