using FirePDF.Model;
using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Runtime.InteropServices;

namespace FirePDF.StreamHelpers
{
    public class RawStreamReader
    {
        public static Bitmap ConvertImageBufferToImage(byte[] buffer, PdfDictionary streamDictionary)
        {
            int width = streamDictionary.Get<int>("Width");
            int height = streamDictionary.Get<int>("Height");
            
            switch (streamDictionary.Get<Name>("ColorSpace"))
            {
                default:
                    throw new NotImplementedException();
            }
        }

        /// <summary>
        /// decompresses a stream from the Pdf stream at the current position and returns it
        /// as the stream isn't compressed, this method has the effect of just copying the bytes to a new stream
        /// </summary>
        public static Stream DecompressStream(Stream pdfStream, PdfDictionary streamDictionary)
        {
            MemoryStream temp = new MemoryStream();

            long length = streamDictionary.Get<int>("Length");

            byte[] buffer = new byte[4096];
            while (length > 0)
            {
                int bytesToRead = (int)Math.Min(buffer.Length, length);
                int bytesRead = pdfStream.Read(buffer, 0, bytesToRead);

                temp.Write(buffer, 0, bytesRead);

                length -= bytesRead;
            }

            temp.Seek(0, SeekOrigin.Begin);

            return temp;
        }

        /// <summary>
        /// takes a buffer containing RGB values and converts it into a System.Drawing.Image
        /// </summary>
        private static Bitmap ConvertRgbArrayToImage(byte[] buffer, int width, int height)
        {
            Bitmap b = new Bitmap(width, height, PixelFormat.Format24bppRgb);

            Rectangle boundsRect = new Rectangle(0, 0, width, height);
            BitmapData bmpData = b.LockBits(boundsRect,
                                            ImageLockMode.WriteOnly,
                                            b.PixelFormat);

            IntPtr ptr = bmpData.Scan0;

            // add back dummy bytes between lines, make each line be a multiple of 4 bytes
            int skipByte = bmpData.Stride - width * 3;
            byte[] newBuff = new byte[buffer.Length + skipByte * height];
            for (int j = 0; j < height; j++)
            {
                Buffer.BlockCopy(buffer, j * width * 3, newBuff, j * (width * 3 + skipByte), width * 3);
            }

            // fill in rgbValues
            Marshal.Copy(newBuff, 0, ptr, newBuff.Length);
            b.UnlockBits(bmpData);

            return b;
        }
    }
}
