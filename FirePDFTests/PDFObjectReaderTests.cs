using Microsoft.VisualStudio.TestTools.UnitTesting;
using FirePDF;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace FirePDF.Tests
{
    [TestClass()]
    public class PDFObjectReaderTests
    {
        [TestMethod()]
        public void readStringTest()
        {
            string test = "(hello world)";
            string expected = "hello world";
            string actual = PDFObjectReader.readString(new MemoryStream(Encoding.ASCII.GetBytes(test)));

            Assert.AreEqual(expected, actual);
        }

        [TestMethod()]
        public void readStringTest2()
        {
            string test = "(hello (world))";
            string expected = "hello (world)";
            string actual = PDFObjectReader.readString(new MemoryStream(Encoding.ASCII.GetBytes(test)));

            Assert.AreEqual(expected, actual);
        }

        [TestMethod()]
        public void readStringTest3()
        {
            string test = @"(hello world\r)";
            string expected = "hello world\r";
            string actual = PDFObjectReader.readString(new MemoryStream(Encoding.ASCII.GetBytes(test)));

            Assert.AreEqual(expected, actual);
        }

        [TestMethod()]
        public void readStringTest4()
        {
            string test = "(hello world\r)";
            string expected = "hello world\n";
            string actual = PDFObjectReader.readString(new MemoryStream(Encoding.ASCII.GetBytes(test)));

            Assert.AreEqual(expected, actual);
        }

        [TestMethod()]
        public void readStringTest5()
        {
            string test = "(hello world\n)";
            string expected = "hello world\n";
            string actual = PDFObjectReader.readString(new MemoryStream(Encoding.ASCII.GetBytes(test)));

            Assert.AreEqual(expected, actual);
        }

        [TestMethod()]
        public void readStringTest6()
        {
            string test = "(hello world\r\n)";
            string expected = "hello world\n";
            string actual = PDFObjectReader.readString(new MemoryStream(Encoding.ASCII.GetBytes(test)));

            Assert.AreEqual(expected, actual);
        }

        [TestMethod()]
        public void readStringTest7()
        {
            string test = @"(hello world\()";
            string expected = "hello world(";
            string actual = PDFObjectReader.readString(new MemoryStream(Encoding.ASCII.GetBytes(test)));

            Assert.AreEqual(expected, actual);
        }

        [TestMethod()]
        public void readStringTest8()
        {
            string test = @"(hello world\012)";
            string expected = "hello world\n";
            string actual = PDFObjectReader.readString(new MemoryStream(Encoding.ASCII.GetBytes(test)));

            Assert.AreEqual(expected, actual);
        }


        [TestMethod()]
        public void readStringTest9()
        {
            string test = @"(hello world\12)";
            string expected = "hello world\n";
            string actual = PDFObjectReader.readString(new MemoryStream(Encoding.ASCII.GetBytes(test)));

            Assert.AreEqual(expected, actual);
        }

        [TestMethod()]
        public void readStringTest10()
        {
            string test = @"(hello world\0123)";
            string expected = "hello world\n3";
            string actual = PDFObjectReader.readString(new MemoryStream(Encoding.ASCII.GetBytes(test)));

            Assert.AreEqual(expected, actual);
        }
    }
}