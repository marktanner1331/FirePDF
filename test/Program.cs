﻿using FirePDF;
using FirePDF.Model;
using FirePDF.Processors;
using FirePDF.Reading;
using FirePDF.Rendering;
using FirePDF.Text;
using FirePDF.Writing;
using Flattener;
using ObjectDumper;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.IO;
using System.Linq;

namespace test
{
    internal class Program
    {
        internal static void Main(string[] args)
        {
            foreach(string file in Directory.EnumerateFiles(@"C:\Users\Mark\Documents\pagesuite\tickets\CS-653 - Sun\orig"))
            {
                turnOffOverprint(file, file.Replace(@"\orig\", @"\fixed\"));
            }
        }

        private static void turnOffOverprint(string input, string output)
        {
            using (Pdf pdf = new Pdf(input))
            {
                Page page = pdf.GetPage(1);

                foreach (IStreamOwner streamOwner in page.listIStreamOwners())
                {
                    foreach (ExtGState extGState in streamOwner.Resources.GetAllExtGStates())
                    {
                        if (extGState.UnderlyingDict.ContainsKey("OP"))
                        {
                            extGState.UnderlyingDict.RemoveEntry("OP");
                        }

                        if (extGState.UnderlyingDict.ContainsKey("op"))
                        {
                            extGState.UnderlyingDict.RemoveEntry("op");
                        }

                        if (extGState.UnderlyingDict.ContainsKey("OPM"))
                        {
                            extGState.UnderlyingDict.RemoveEntry("OPM");
                        }
                    }
                }

                pdf.Save(output, SaveType.Update);
            }
        }
    }
}