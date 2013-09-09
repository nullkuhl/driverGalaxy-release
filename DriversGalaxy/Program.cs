using System;
using System.Diagnostics;

namespace DriversGalaxy
{
    class Program
    {
        [STAThread]
        public static void Main(string[] args)
        {
            WindowsFormsApp wrapper = new WindowsFormsApp();
            wrapper.Run(args);
        }

      
    }
}
