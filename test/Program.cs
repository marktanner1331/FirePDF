using FirePDF;
using FirePDF.Model;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace test
{
    class Program
    {
        static void Main(string[] args)
        {
            string imageFile = @"C:\Users\Mark Tanner\Pictures\image_2019_09_16T13_30_05_940Z.png";

            using (PDF pdf = new PDF())
            {
                XObjectImage image = XObjectImage.fromFile(pdf, imageFile);

            }

        }
    }
}
