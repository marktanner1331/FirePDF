using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace FirePDF
{
    public static class PDFReaderLevel1
    {
        public static long readLastStartXREF(string chunk)
        {
            int offset = chunk.LastIndexOf("startxref");
            chunk = chunk.Substring(offset + 9);

            return long.Parse(Regex.Match(chunk, @"(\d+)").Groups[1].Value);
        }
    }
}
