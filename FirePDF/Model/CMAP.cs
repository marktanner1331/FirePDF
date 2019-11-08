using FirePDF.Reading;
using FirePDF.Util;
using FirePDF.Writing;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FirePDF.Model
{
    //TODO: change every char to an int
    public class CMAP
    {
        public int wMode;
        public Name name;
        public int type;
        public string registry;
        public string ordering;
        public int supplement;
        public string version;

        private int minCodeLength = 4;
        private int maxCodeLength = 0;

        private List<CodeSpaceRange> codeSpaceRanges;

        //some codes are mapped to CID's individually and are stored here
        private Dictionary<int, int> cidMap;

        //others are stored as part of CID ranges and are stored here
        private List<CIDRange> cidRanges;

        //value must be a string as some codes map to multiple chars (e.g. ligatures)
        private Dictionary<int, string> codeToUnicodeMap;

        public CMAP()
        {
            codeSpaceRanges = new List<CodeSpaceRange>();
            cidMap = new Dictionary<int, int>();
            cidRanges = new List<CIDRange>();
            codeToUnicodeMap = new Dictionary<int, string>();
        }

        public string codeToUnicode(int code)
        {
            if(codeToUnicodeMap.ContainsKey(code))
            {
                return codeToUnicodeMap[code];
            }

            return null;
        }

        public int codeToCID(int code)
        {
            if(cidMap.ContainsKey(code))
            {
                return cidMap[code];
            }
            
            foreach (CIDRange range in cidRanges)
            {
                int cid = range.codeToCID(code);
                if (cid != -1)
                {
                    return cid;
                }
            }

            return 0;
        }

        /// <summary>
        /// reads a character code from the stream
        /// </summary>
        public int readCodeFromStream(Stream stream)
        {
            //we create a buffer that stores our code
            byte[] bytes = new byte[maxCodeLength];

            //we only read in minCodeLength - 1 bytes at the moment
            //as each iteration of the below loop will read in one more byte as the size of 'codeLength' increases
            stream.Read(bytes, 0, minCodeLength - 1);
            
            for (int codeLength = minCodeLength; codeLength <= maxCodeLength; codeLength++)
            {
                //if codeLength is 4 then we have read in the first 3 bytes already
                //meaning we need to read in the 4th byte
                //which has position '3' in the array
                //i.e. codeLength - 1
                bytes[codeLength - 1] = (byte)stream.ReadByte();

                foreach (CodeSpaceRange range in codeSpaceRanges)
                {
                    if (range.codeLength == codeLength)
                    {
                        int code = ByteReader.readBigEndianInt(bytes, codeLength);
                        if (range.isInRange(code))
                        {
                            return code;
                        }
                    }
                }
            }

            Logger.warning("Invalid character code sequence: " + ByteWriter.byteArrayToHexString(bytes) + "in CMap: " + name);
            return 0;
        }

        public void addCodeSpaceRange(CodeSpaceRange range)
        {
            codeSpaceRanges.Add(range);
            maxCodeLength = Math.Max(maxCodeLength, range.codeLength);
            minCodeLength = Math.Min(minCodeLength, range.codeLength);
        }

        public void addCIDMapping(char code, int cid)
        {
            cidMap[code] = cid;
        }

        /// <summary>
        /// adds the given CID range to the set of CID ranges covered by this CMAP
        /// </summary>
        public void addCIDRange(char start, char end, int cid)
        {
            //first we check if the most recent range can be extended to cover the new range
            if(cidRanges.Count != 0)
            {
                if(cidRanges.Last().tryExtend(start, end, cid))
                {
                    return;
                }
            }

            //otherwise add a new range
            cidRanges.Add(new CIDRange(start, end, cid));
        }

        public void addCharMapping(int code, string unicode)
        {
            codeToUnicodeMap.Add(code, unicode);
        }
    }
}
