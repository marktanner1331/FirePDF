using FirePDF;
using FirePDF.Model;
using FirePDF.Processors;
using FirePDF.Reading;
using FirePDF.Writing;
using FirePDF.Modifying;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace Samples
{
    //TODO:
    //  OPI dictionaries
    //  Patterns can have their own ExtGStates

    class TurnOffOverPrinting
    {
        internal static void Main(string[] args)
        {
            const string file = @"C:\Users\Mark\Documents\pagesuite\tickets\CS-816 - Racing Post\orig.pdf";
            using (Pdf pdf = new Pdf(file))
            {
                Page page = pdf.GetPage(1);
                
                foreach(IStreamOwner streamOwner in page.listIStreamOwners())
                {
                    foreach(ExtGState extGState in streamOwner.Resources.GetAllExtGStates())
                    {
                        if(extGState.UnderlyingDict.ContainsKey("OP"))
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
                
                pdf.Save(@"C:\Users\Mark\Documents\pagesuite\tickets\CS-816 - Racing Post\orig fixed.pdf", SaveType.Update);
            }
        }
    }
}
