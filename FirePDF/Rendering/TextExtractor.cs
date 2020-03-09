using FirePDF.Model;
using FirePDF.Processors;
using FirePDF.Reading;
using System;
using System.Collections.Generic;
using System.Drawing.Drawing2D;
using System.Text;

namespace FirePDF.Rendering
{
    public class TextExtractor : IRenderer
    {
        private List<string> extractedText;

        public TextExtractor()
        {
            extractedText = new List<string>();
        }

        public override void drawImage(XObjectImage image)
        {

        }

        public override void drawText(byte[] text)
        {
            Name baseFont = getGraphicsState().font.baseFont;
            if (baseFont == "HGEBGL+News706BT-RomanC")
            {
                string unicode = getGraphicsState().font.readUnicodeStringFromHexString(text);
                extractedText.Add(unicode);
                if (unicode.Contains("therapist"))
                {
                    unicode = getGraphicsState().font.readUnicodeStringFromHexString(text);
                }
            }
        }

        public void extractText(Page page)
        {
            extractedText.Clear();

            StreamProcessor processor = new StreamProcessor(this);
            RecursiveStreamReader reader = new RecursiveStreamReader(processor);
            reader.readStreamRecursively(page);
        }

        public override void fillAndStrokePath(GraphicsPath path)
        {

        }

        public override void fillPath(GraphicsPath path)
        {

        }

        public override void strokePath(GraphicsPath path)
        {

        }
    }
}
