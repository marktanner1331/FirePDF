using FirePDF;
using FirePDF.Processors;
using FirePDF.Reading;
using FirePDF.Rendering;

namespace Flattener
{
    public class Program
    {
        public static void Main(string[] args)
        {
            const string file = @"C:\Users\Mark Tanner\scratch\sarie page 1.Pdf";
            Pdf pdf = new Pdf(file);

            Page page = pdf.GetPage(1);
            
            Renderer renderer = new Splitter();

            StreamProcessor sp = new StreamProcessor(renderer);
            RecursiveStreamReader streamReader = new RecursiveStreamReader(sp);

            streamReader.ReadStreamRecursively(page);
        }
    }
}
