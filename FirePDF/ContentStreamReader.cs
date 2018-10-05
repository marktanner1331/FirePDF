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
        public static void readContentStream(Stream decompressedStream)
        {
            StreamReader reader = new StreamReader(decompressedStream);
            string commands = reader.ReadToEnd();
            
        }
    }
}
