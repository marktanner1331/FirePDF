using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FirePDF
{
    public abstract class PDFContentStream
    {
        public abstract Stream readStream();
    }
}
