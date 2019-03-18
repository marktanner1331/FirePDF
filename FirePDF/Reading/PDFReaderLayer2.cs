using FirePDF.Model;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FirePDF.Reading
{
    public static class PDFReaderLayer2
    {
        public static Stream readContentStream(Page page)
        {
            //TODO move this as a method of the xObject

            MemoryStream compositeStream = new MemoryStream();

            List<object> contents = (List<object>)page.underlyingDict["Contents"];
            if (contents == null)
            {
                throw new Exception();
            }
            
            foreach (ObjectReference objectReference in contents)
            {
                using (Stream stream = PDFReaderLayer1.readContentStream(page.pdf, objectReference))
                {
                    stream.CopyTo(compositeStream);
                }
            }

            compositeStream.Position = 0;
            return compositeStream;
        }

        public static Stream readContentStream(XObjectForm xObject)
        {
            //TODO move this as a method of the xObject
            return PDFReaderLayer1.readContentStream(xObject.pdf, xObject.underlyingDict, xObject.startOfStream);
        }
    }
}
