using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FirePDF
{
    public class PNGPredictor
    {
        public static byte[] decompress(byte[] compressedBytes, int columns)
        {
            //each row has an extra byte at its start for the predictor
            if(compressedBytes.Length % (columns + 1) != 0)
            {
                //row length == columns + 1
                throw new Exception("byte array length must be a multiple of the row length");
            }

            int rows = compressedBytes.Length / (columns + 1);

            //remove the extra byte per row
            byte[] decompressedBytes = new byte[compressedBytes.Length - rows];

            int compressedOffset = 0;
            int decompressedOffset = 0;

            byte[] previousRow = new byte[columns];
            
            while(compressedOffset < compressedBytes.Length)
            {
                int predictor = compressedBytes[compressedOffset];
                compressedOffset++;

                if (predictor == -1)
                {
                    throw new Exception("tried to read past the end of the stream");
                }

                predictor += 10;

                byte[] nextRow = new byte[columns];
                Array.Copy(compressedBytes, compressedOffset, nextRow, 0, columns);
                compressedOffset += columns;

                switch (predictor)
                {
                    case 12:
                        // PRED UP
                        for (int p = 0; p < columns; p++)
                        {
                            int up = nextRow[p] & 0xff;
                            int prior = previousRow[p] & 0xff;
                            decompressedBytes[decompressedOffset] = previousRow[p] = (byte)((up + prior) & 0xff);
                            
                            decompressedOffset++;
                        }
                        break;
                    default:
                        throw new NotImplementedException();
                }
            }

            return decompressedBytes;
        }
    }
}
