using FirePDF.Reading;
using FirePDF.Writing;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace FirePDF.Model
{
    public class Trailer : IHaveUnderlyingDict
    {
        public int? size
        {
            get => underlyingDict.get<int?>("Size");
            set => underlyingDict.set("Size", value);
        }

        public ObjectReference root
        {
            get => underlyingDict.get<ObjectReference>("Root");
            set => underlyingDict.set("Root", value);
        }

        public int? prev
        {
            get => underlyingDict.get<int?>("Prev");
            set => underlyingDict.set("Prev", value);
        }
        
        public int? XRefStm
        {
            get => underlyingDict.get<int?>("XRefStm");
            set => underlyingDict.set("XRefStm", value);
        }

        public Trailer(PDF pdf) : base(new PDFDictionary(pdf))
        {

        }

        public Trailer(PDFDictionary underylingDict) : base(underylingDict)
        {
            
        }

        public void serialize(PDFWriter writer)
        {
            writer.writeASCII("trailer");
            writer.writeNewLine();
            
            writer.writeDirectObject(underlyingDict);
            writer.writeNewLine();
        }

    }
}