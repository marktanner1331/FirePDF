using FirePDF.Model;
using FirePDF.Processors;
using FirePDF.Reading;
using System.Collections.Generic;
using System.Drawing.Drawing2D;

namespace FirePDF.Rendering
{
    public class TextExtractor : Renderer
    {
        private readonly List<string> extractedText;

        public TextExtractor()
        {
            extractedText = new List<string>();
        }

        public override void DrawImage(XObjectImage image)
        {

        }

        public override void DrawText(byte[] text)
        {
            Name baseFont = getGraphicsState().font.baseFont;
            if (baseFont.ToString().Contains("News706BT-RomanC") && getGraphicsState().font.Encoding != null)
            {
                string unicode = getGraphicsState().font.ReadUnicodeStringFromHexString(text);
                extractedText.Add(unicode);
                
            }
        }

        public void ExtractText(Page page)
        {
            extractedText.Clear();

            StreamProcessor processor = new StreamProcessor(this);
            RecursiveStreamReader reader = new RecursiveStreamReader(processor);
            reader.ReadStreamRecursively(page);
        }

        public override void FillAndStrokePath(GraphicsPath path)
        {

        }

        public override void FillPath(GraphicsPath path)
        {

        }

        public override void StrokePath(GraphicsPath path)
        {

        }
    }
}
