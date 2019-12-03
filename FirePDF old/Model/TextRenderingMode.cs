using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FirePDF.Model
{
    public enum TextRenderingMode
    {
        FillText = 0,
        StrokeText = 1,
        FillThenStrokeText = 2,
        DontFillOrStrokeText = 3,
        FillTextAndClipPath = 4,
        StrokeTextAndClipPath = 5,
        FillThenStrokeTextAndClipPath = 6,
        DontFillOrStrokeTextAndClipPath = 7
    }
}
