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
    class SoftMask
    {
        private readonly Stream stream;
        private readonly PDFDictionary underlyingDict;
        private readonly int maskWidth;
        private readonly int maskHeight;

        public SoftMask(XObjectImage image) : this(image.getRawStream(), image.underlyingDict)
        {
        }

        public SoftMask(Stream stream, PDFDictionary underlyingDict)
        {
            this.stream = stream;
            this.underlyingDict = underlyingDict;

        }

        public Bitmap applyMaskToImage(Bitmap image)
        {
            BinaryReader br = new BinaryReader(stream);
            byte[] bytes = br.ReadBytes((int)stream.Length);

            Bitmap output = new Bitmap(image.Width, image.Height, PixelFormat.Format32bppArgb);
            output.MakeTransparent();
            var rect = new Rectangle(0, 0, image.Width, image.Height);

            var bitsImage = image.LockBits(rect, ImageLockMode.ReadOnly, PixelFormat.Format24bppRgb);
            var bitsOutput = output.LockBits(rect, ImageLockMode.WriteOnly, PixelFormat.Format32bppArgb);

            unsafe
            {
                fixed (byte* maskBuffer = bytes)
                {
                    for (int y = 0; y < image.Height; y++)
                    {
                        byte* ptrMask = maskBuffer + y * maskWidth;
                        byte* ptrInput = (byte*)bitsImage.Scan0 + y * bitsImage.Stride;
                        byte* ptrOutput = (byte*)bitsOutput.Scan0 + y * bitsOutput.Stride;
                        for (int x = 0; x < image.Width; x++)
                        {
                            if (ptrMask[x] == 0)
                            {
                                ptrOutput[4 * x] = ptrInput[4 * x]; // blue
                                ptrOutput[4 * x + 1] = ptrInput[4 * x + 1]; // green
                                ptrOutput[4 * x + 2] = ptrInput[4 * x + 2]; // red

                                //Ensure opaque
                                ptrOutput[4 * x + 3] = 255;
                            }
                            else
                            {
                                ptrOutput[4 * x] = 0; // blue
                                ptrOutput[4 * x + 1] = 0; // green
                                ptrOutput[4 * x + 2] = 0; // red

                                //Ensure Transparent
                                ptrOutput[4 * x + 3] = 0; // alpha
                            }
                        }
                    }
                }
            }

            image.UnlockBits(bitsImage);
            output.UnlockBits(bitsOutput);

            return output;
        }
    }
}
