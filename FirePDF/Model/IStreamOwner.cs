using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FirePDF.Model
{
    public interface IStreamOwner
    {
        PDFResources resources { get; }
        RectangleF boundingBox { get; }
        PDF pdf { get; }

        Stream getStream();
    }
}
