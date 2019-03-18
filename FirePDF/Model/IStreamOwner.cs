﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FirePDF.Model
{
    public interface IStreamOwner
    {
        PDFResources resources { get; }
        Rectangle boundingBox { get; }
    }
}
