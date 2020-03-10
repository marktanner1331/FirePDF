using System.IO;
using System.Text;
using FirePDF.Reading;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace FirePDFTests
{
    [TestClass()]
    public class PdfObjectReaderTests
    {
        [TestMethod()]
        public void ReadStringTest()
        {
            const string test = "(hello world)";
            const string expected = "hello world";
            string actual = PdfReader.ReadString(new MemoryStream(Encoding.ASCII.GetBytes(test))).ToString();

            Assert.AreEqual(expected, actual);
        }

        [TestMethod()]
        public void ReadStringTest2()
        {
            const string test = "(hello (world))";
            const string expected = "hello (world)";
            string actual = PdfReader.ReadString(new MemoryStream(Encoding.ASCII.GetBytes(test))).ToString();

            Assert.AreEqual(expected, actual);
        }

        [TestMethod()]
        public void ReadStringTest3()
        {
            const string test = @"(hello world\r)";
            const string expected = "hello world\r";
            string actual = PdfReader.ReadString(new MemoryStream(Encoding.ASCII.GetBytes(test))).ToString();

            Assert.AreEqual(expected, actual);
        }

        [TestMethod()]
        public void ReadStringTest4()
        {
            const string test = "(hello world\r)";
            const string expected = "hello world\n";
            string actual = PdfReader.ReadString(new MemoryStream(Encoding.ASCII.GetBytes(test))).ToString();

            Assert.AreEqual(expected, actual);
        }

        [TestMethod()]
        public void ReadStringTest5()
        {
            const string test = "(hello world\n)";
            const string expected = "hello world\n";
            string actual = PdfReader.ReadString(new MemoryStream(Encoding.ASCII.GetBytes(test))).ToString();

            Assert.AreEqual(expected, actual);
        }

        [TestMethod()]
        public void ReadStringTest6()
        {
            const string test = "(hello world\r\n)";
            const string expected = "hello world\n";
            string actual = PdfReader.ReadString(new MemoryStream(Encoding.ASCII.GetBytes(test))).ToString();

            Assert.AreEqual(expected, actual);
        }

        [TestMethod()]
        public void ReadStringTest7()
        {
            const string test = @"(hello world\()";
            const string expected = "hello world(";
            string actual = PdfReader.ReadString(new MemoryStream(Encoding.ASCII.GetBytes(test))).ToString();

            Assert.AreEqual(expected, actual);
        }

        [TestMethod()]
        public void ReadStringTest8()
        {
            const string test = @"(hello world\012)";
            const string expected = "hello world\n";
            string actual = PdfReader.ReadString(new MemoryStream(Encoding.ASCII.GetBytes(test))).ToString();

            Assert.AreEqual(expected, actual);
        }


        [TestMethod()]
        public void ReadStringTest9()
        {
            const string test = @"(hello world\12)";
            const string expected = "hello world\n";
            string actual = PdfReader.ReadString(new MemoryStream(Encoding.ASCII.GetBytes(test))).ToString();

            Assert.AreEqual(expected, actual);
        }

        [TestMethod()]
        public void ReadStringTest10()
        {
            const string test = @"(hello world\0123)";
            const string expected = "hello world\n3";
            string actual = PdfReader.ReadString(new MemoryStream(Encoding.ASCII.GetBytes(test))).ToString();

            Assert.AreEqual(expected, actual);
        }
    }
}