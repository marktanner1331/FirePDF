﻿using Microsoft.VisualStudio.TestTools.UnitTesting;
using FirePDF;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Reflection;
using System.IO;
using System.Drawing;
using FirePDF.Reading;
using FirePDF.Model;

namespace FirePDF.Tests
{
    [TestClass()]
    public class PageTests
    {
        private string getPDFFolder()
        {
            return System.IO.Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) + "/../../pdfs/";
        }

        [TestMethod()]
        public void getImagesTest()
        {
            string file = getPDFFolder() + "pb13332-cop-cats-091204.pdf";
            PDF pdf = new PDF(file);

            Page page = pdf.getPage(1);
            Assert.AreEqual(1, page.resources.listXObjectImages().Count());
        }

        [TestMethod()]
        public void readFormTest()
        {
            string file = getPDFFolder() + "page 24 fixed.pdf";
            PDF pdf = new PDF(file);

            Page page = pdf.getPage(1);

            List<string> forms = page.resources.listXObjectForms().ToList();
            XObjectForm form = page.resources.getXObjectForm(forms.First());

            Stream s = form.getStream();
            //List<Operation> operations = ContentStreamReader.readOperationsFromStream(s);

            //Assert.AreEqual(601, operations.Count);
        }

        [TestMethod()]
        public void getContentStreamTest()
        {
            string file = getPDFFolder() + "pb13332-cop-cats-091204.pdf";
            PDF pdf = new PDF(file);

            Page page = pdf.getPage(1);
            Stream s = page.getStream();

            using (System.IO.StreamReader streamReader = new System.IO.StreamReader(s))
            {
                string str = streamReader.ReadToEnd();
            }
        }
    }
}