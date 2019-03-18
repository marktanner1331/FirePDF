using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FirePDF.StreamHelpers
{
    public class PNGPredictor
    {
        public static byte[] decompress(byte[] compressedBytes, int columns, int bytesPerPixel)
        {
            int rowLength = columns * bytesPerPixel;

            //each row has an extra byte at its start for the predictor
            if (compressedBytes.Length % (rowLength + 1) != 0)
            {
                //row length == columns + 1
                throw new Exception("byte array length must be a multiple of the row length");
            }

            int rows = compressedBytes.Length / (rowLength + 1);

            //remove the extra byte per row
            byte[] decompressedBytes = new byte[compressedBytes.Length - rows];

            int compressedOffset = 0;
            int decompressedOffset = 0;

            byte[] previousRow = new byte[rowLength];

            while (compressedOffset < compressedBytes.Length)
            {
                int predictor = compressedBytes[compressedOffset];
                compressedOffset++;

                if (predictor == -1)
                {
                    throw new Exception("tried to read past the end of the stream");
                }

                predictor += 10;

                byte[] nextRow = new byte[rowLength];
                Array.Copy(compressedBytes, compressedOffset, nextRow, 0, rowLength);
                compressedOffset += rowLength;

                for (int p = 0; p < rowLength; p++)
                {
                    int up;
                    int prior;

                    switch (predictor)
                    {
                        case 10:
                            up = nextRow[p] & 0xff;
                            prior = 0;
                            break;
                        case 11:
                            // PRED SUB
                            up = nextRow[p] & 0xff;
                            prior = p >= bytesPerPixel ? previousRow[p - bytesPerPixel] & 0xff : 0;
                            break;
                        case 12:
                            // PRED UP
                            up = nextRow[p] & 0xff;
                            prior = previousRow[p] & 0xff;
                            break;
                        default:
                            throw new NotImplementedException();
                    }

                    decompressedBytes[decompressedOffset] = previousRow[p] = (byte)((up + prior) & 0xff);
                    decompressedOffset++;
                }
            }

            return decompressedBytes;
        }
    }
}
