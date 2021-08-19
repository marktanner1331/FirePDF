using System.Drawing;
using System.IO;

namespace FirePDF.Model
{
    public interface IStreamOwner
    {
        PdfResources Resources { get; }
        RectangleF BoundingBox { get; }
        Pdf Pdf { get; }
        PdfDictionary UnderlyingDict { get; }

        Stream GetStream();
    }
}
