using System.Drawing;
using System.IO;

namespace FirePDF.Model
{
    public interface IStreamOwner
    {
        PdfResources Resources { get; }
        RectangleF BoundingBox { get; }
        Pdf Pdf { get; }

        Stream GetStream();
    }
}
