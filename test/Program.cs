using FirePDF;
using FirePDF.Model;
using FirePDF.Processors;
using FirePDF.Reading;
using FirePDF.Rendering;
using FirePDF.StreamPartFunctions;
using FirePDF.Text;
using FirePDF.Writing;
using Flattener;
using ObjectDumper;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace test
{
    internal class Program
    {
        internal static void Main(string[] args)
        {
            string file = @"C:\Users\Mark\Documents\pagesuite\tickets\CS-653 - Sun\filtered 10 clean.pdf";

            using(Pdf pdf = new Pdf(file))
            {
                Page page = pdf.GetPage(1);
                List<Operation> operations = ContentStreamReader.ReadOperationsFromStream(pdf, page.GetStream());

                page.GetStream();
            }
        }

        static void cleanStream(IStreamOwner streamOwner)
        {
            List<ObjectReference> formsToExplore = new List<ObjectReference>();
            HashSet<string> usedResources = new HashSet<string>();

            List<Operation> operations = ContentStreamReader.ReadOperationsFromStream(streamOwner.Pdf, streamOwner.GetStream());
            foreach (Operation operation in operations)
            {
                if (operation.operatorName == "Do" && streamOwner.Resources.IsXObjectForm((Name)operation.operands[0]))
                {
                    ObjectReference formRef = (ObjectReference)streamOwner.Resources.GetUnresolvedObjectAtPath("XObject", operation.GetOperandAsName(0));
                    formsToExplore.Add(formRef);
                }

                switch (operation.operatorName)
                {
                    case "Do":
                        usedResources.Add("XObject/" + operation.GetOperandAsName(0));
                        break;
                    case "gs":
                        usedResources.Add("ExtGState/" + operation.GetOperandAsName(0));
                        break;
                    case "Tf":
                        usedResources.Add("Font/" + operation.GetOperandAsName(0));
                        break;
                }
            }

            foreach(string resource in streamOwner.Resources.enumerateResourcePaths().ToList())
            {
                switch(resource.Split('/')[0])
                {
                    case "Font":
                    case "XObject":
                    case "ExtGState":
                        break;
                    default:
                        continue;
                }

                if(usedResources.Contains(resource) == false)
                {
                    string[] split = resource.Split('/');
                    streamOwner.Resources.removeResource(split[0], split[1]);
                }
            }

            foreach(ObjectReference form in formsToExplore)
            {
                object obj = form.Get<object>();
                cleanStream((XObjectForm)obj);
            }
        }
    }
}