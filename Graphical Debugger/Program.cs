﻿using System;
using System.Windows.Forms;

namespace Graphical_Debugger
{
    internal static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        private static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new MainWindow(@"C:\Users\Mark\Documents\pagesuite\tickets\231591\trueorig.pdf"));
        }
    }
}
