﻿using FirePDF;
using FirePDF.Distilling;
using FirePDF.Model;
using FirePDF.Processors;
using FirePDF.Reading;
using FirePDF.Rendering;
using FirePDF.StreamPartFunctions;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace test
{
    class Program
    {
        static void Main(string[] args)
        {
            string file = @"C:\Users\Mark Tanner\scratch\page 24 fixed.pdf";
            PDF pdf = new PDF(file);

            Page page = pdf.getPage(1);

            List<XObjectForm> forms = page.resources.getXObjectForms().ToList();
            XObjectForm form = forms.First();

            Stream s = form.readContentStream();
            List<Operation> operations = ContentStreamReader.readOperationsFromStream(s);

            StreamTree tree = new StreamTree(operations);
            Debug.WriteLine(tree.toVerboseString());
        }
    }
}
