using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;

namespace Graphic
{
    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new Form1());
            Console.Clear();
            Console.WriteLine("Do you really want to EXIT?\nPress Enter");
            //Console.ReadKey();
        }
    }
}
