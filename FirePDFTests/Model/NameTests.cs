using Microsoft.VisualStudio.TestTools.UnitTesting;
using FirePDF.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FirePDF.Model.Tests
{
    [TestClass()]
    public class NameTests
    {
        [TestMethod()]
        public void NameTest()
        {
            string s = "asdf";
            Name n = s;

            s = n;

            Dictionary<string, string> myDict = new Dictionary<string, string>();
            myDict[n] = s;
            s = myDict[n];

            myDict[n] = n;
            n = myDict[n];

            Dictionary<Name, object> myDict2 = new Dictionary<Name, object>();
            myDict2["test"] = "sadf";
            myDict2["test2"] = (Name)"asdf";

            string t = "asdf";
            object o = t;
            Assert.IsFalse(o is Name);
        }
    }
}