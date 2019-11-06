﻿using FirePDF.Reading;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FirePDF.Model
{
    public class XObjectImage : PDFStream
    {
        /// <summary>
        /// initializing an xobject image with the owning pdf, its dictionary, and the offset to the start of the stream relative to the start of the pdf
        /// </summary>
        public XObjectImage(PDF pdf, PDFDictionary dict, long startOfStream) : base(pdf, dict, startOfStream)
        {
            
        }

        public Bitmap getImage()
        {
            pdf.stream.Position = startOfStream;
            Bitmap image = PDFReader.decompressImageStream(pdf, pdf.stream, underlyingDict);

            if(underlyingDict.ContainsKey("SMask"))
            {
                XObjectImage mask = underlyingDict.get<XObjectImage>("SMask");
                //DoApplyMask(image, mask);
            }

            return image;
        }

    }
}
