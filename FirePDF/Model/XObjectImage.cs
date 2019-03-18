﻿using FirePDF.Reading;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FirePDF.Model
{
    public class XObjectImage
    {
        private PDF pdf;
        private Dictionary<string, object> underlyingDict;
        public long startOfStream;

        public XObjectImage(PDF pdf)
        {
            this.pdf = pdf;

            //need to initialize a new underlyingDict and stream
            throw new NotImplementedException();
        }

        /// <summary>
        /// initializing an xobject image with the owning pdf, its dictionary, and the offset to the start of the stream relative to the start of the pdf
        /// </summary>
        public XObjectImage(PDF pdf, Dictionary<string, object> dict, long startOfStream)
        {
            this.pdf = pdf;
            this.underlyingDict = dict;
            this.startOfStream = startOfStream;
        }

        public Image getImage()
        {
            pdf.stream.Position = startOfStream;
            return PDFReader.decompressImageStream(pdf.stream, underlyingDict);
        }
    }
}
