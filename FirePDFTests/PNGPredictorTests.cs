﻿using System.Linq;
using FirePDF.StreamHelpers;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace FirePDFTests
{
    [TestClass()]
    public class PngPredictorTests
    {
        [TestMethod()]
        public void ReadTest()
        {
            byte[] compressed = { 0x02, 0x01, 0x00, 0x00, 0x10, 0x00, 0x02, 0x00, 0x00, 0x02, 0xBA, 0x00, 0x02, 0x00, 0x00, 0x01, 0xD6, 0x00, 0x02, 0x00, 0x00, 0x01, 0xAD, 0x00, 0x02, 0x00, 0x00, 0x0F, 0xB3, 0x00, 0x02, 0x00, 0x00, 0x07, 0x44, 0x00, 0x02, 0x00, 0x00, 0x00, 0x65, 0x00, 0x02, 0x00, 0x01, 0x75, 0x63, 0x00, 0x02, 0x00, 0x00, 0x02, 0x95, 0x00, 0x02, 0x00, 0x00, 0x06, 0x29, 0x00, 0x02, 0x00, 0x00, 0x00, 0x1C, 0x00, 0x02, 0x01, 0xFF, 0x69, 0x2A, 0x00, 0x02, 0x00, 0x00, 0x00, 0x00, 0x01, 0x02, 0x00, 0x00, 0x00, 0x00, 0x01, 0x02, 0x00, 0x00, 0x00, 0x00, 0x01, 0x02, 0x00, 0x00, 0x00, 0x00, 0x01, 0x02, 0x00, 0x00, 0x00, 0x00, 0x01, 0x02, 0x00, 0x00, 0x00, 0x00, 0x01, 0x02, 0x00, 0x00, 0x00, 0x00, 0x01, 0x02, 0x00, 0x00, 0x00, 0x00, 0x01, 0x02, 0xFF, 0x00, 0x03, 0xF5, 0xF8, 0x02, 0x00, 0x01, 0xE5, 0x33, 0x00, 0x02, 0x00, 0xFF, 0x18, 0x7E, 0x00 };
            byte[] expected = { 0x01, 0x00, 0x00, 0x10, 0x00, 0x01, 0x00, 0x02, 0xCA, 0x00, 0x01, 0x00, 0x03, 0xA0, 0x00, 0x01, 0x00, 0x04, 0x4D, 0x00, 0x01, 0x00, 0x13, 0x00, 0x00, 0x01, 0x00, 0x1A, 0x44, 0x00, 0x01, 0x00, 0x1A, 0xA9, 0x00, 0x01, 0x01, 0x8F, 0x0C, 0x00, 0x01, 0x01, 0x91, 0xA1, 0x00, 0x01, 0x01, 0x97, 0xCA, 0x00, 0x01, 0x01, 0x97, 0xE6, 0x00, 0x02, 0x00, 0x00, 0x10, 0x00, 0x02, 0x00, 0x00, 0x10, 0x01, 0x02, 0x00, 0x00, 0x10, 0x02, 0x02, 0x00, 0x00, 0x10, 0x03, 0x02, 0x00, 0x00, 0x10, 0x04, 0x02, 0x00, 0x00, 0x10, 0x05, 0x02, 0x00, 0x00, 0x10, 0x06, 0x02, 0x00, 0x00, 0x10, 0x07, 0x02, 0x00, 0x00, 0x10, 0x08, 0x01, 0x00, 0x03, 0x05, 0x00, 0x01, 0x01, 0xE8, 0x38, 0x00, 0x01, 0x00, 0x00, 0xB6, 0x00 };

            byte[] decompressed = PngPredictor.Decompress(compressed, 5, 1);
           
            Assert.IsTrue(Enumerable.SequenceEqual(expected, decompressed));
        }

        [TestMethod()]
        public void ReadTest2()
        {
            byte[] compressed = { 0x02, 0x01, 0x00, 0x00, 0x10, 0x00, 0x02, 0x00, 0x00, 0x02, 0xBA, 0x00, 0x02, 0x00, 0x00, 0x01, 0xD6, 0x00, 0x02, 0x00, 0x00, 0x01, 0xAD, 0x00, 0x02, 0x00, 0x00, 0x0F, 0xB3, 0x00, 0x02, 0x00, 0x00, 0x07, 0x44, 0x00, 0x02, 0x00, 0x00, 0x00, 0x65, 0x00, 0x02, 0x00, 0x01, 0x75, 0x63, 0x00, 0x02, 0x00, 0x00, 0x02, 0x95, 0x00, 0x02, 0x00, 0x00, 0x06, 0x29, 0x00, 0x02, 0x00, 0x00, 0x00, 0x1C, 0x00, 0x02, 0x01, 0xFF, 0x69, 0x2A, 0x00, 0x02, 0x00, 0x00, 0x00, 0x00, 0x01, 0x02, 0x00, 0x00, 0x00, 0x00, 0x01, 0x02, 0x00, 0x00, 0x00, 0x00, 0x01, 0x02, 0x00, 0x00, 0x00, 0x00, 0x01, 0x02, 0x00, 0x00, 0x00, 0x00, 0x01, 0x02, 0x00, 0x00, 0x00, 0x00, 0x01, 0x02, 0x00, 0x00, 0x00, 0x00, 0x01, 0x02, 0x00, 0x00, 0x00, 0x00, 0x01, 0x02, 0xFF, 0x00, 0x03, 0xF5, 0xF8, 0x02, 0x00, 0x01, 0xE5, 0x33, 0x00, 0x02, 0x00, 0xFF, 0x18, 0x7E, 0x00 };
            byte[] expected = { 0x01, 0x00, 0x00, 0x10, 0x00, 0x01, 0x00, 0x02, 0xCA, 0x00, 0x01, 0x00, 0x03, 0xA0, 0x00, 0x01, 0x00, 0x04, 0x4D, 0x00, 0x01, 0x00, 0x13, 0x00, 0x00, 0x01, 0x00, 0x1A, 0x44, 0x00, 0x01, 0x00, 0x1A, 0xA9, 0x00, 0x01, 0x01, 0x8F, 0x0C, 0x00, 0x01, 0x01, 0x91, 0xA1, 0x00, 0x01, 0x01, 0x97, 0xCA, 0x00, 0x01, 0x01, 0x97, 0xE6, 0x00, 0x02, 0x00, 0x00, 0x10, 0x00, 0x02, 0x00, 0x00, 0x10, 0x01, 0x02, 0x00, 0x00, 0x10, 0x02, 0x02, 0x00, 0x00, 0x10, 0x03, 0x02, 0x00, 0x00, 0x10, 0x04, 0x02, 0x00, 0x00, 0x10, 0x05, 0x02, 0x00, 0x00, 0x10, 0x06, 0x02, 0x00, 0x00, 0x10, 0x07, 0x02, 0x00, 0x00, 0x10, 0x08, 0x01, 0x00, 0x03, 0x05, 0x00, 0x01, 0x01, 0xE8, 0x38, 0x00, 0x01, 0x00, 0x00, 0xB6, 0x00 };

            byte[] decompressed = PngPredictor.Decompress(compressed, 5, 1);
            Assert.AreEqual(expected.Length, decompressed.Length);

            for (int i = 0; i < expected.Length; i++)
            {
                Assert.AreEqual(expected[i], decompressed[i]);
            }
        }
    }
}