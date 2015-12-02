using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Autoupdate
{
    public class Program
    {


        [DllImport("user32.dll", SetLastError = true, CharSet = CharSet.Unicode)]
        private static extern IntPtr FindWindow(string lpClassName, string lpWindowName);

        private static IntPtr FindShinraMeterWindow()
        {
            var error0 = Marshal.GetLastWin32Error();
            var result = FindWindow(null, "ShinraMeter");
            var error = Marshal.GetLastWin32Error();
            //if (result == IntPtr.Zero && (error != 0))
            //    throw new Win32Exception();
            return result;
        }

        static void Main(string[] args)
        {
            bool aIsNewInstance, isUpdating;
             Mutex _unique;
            _unique = new Mutex(true, "ShinraMeter", out aIsNewInstance);
            while (!aIsNewInstance)
            {
                Console.WriteLine("Sleep");
                Thread.Sleep(1000);
               _unique = new Mutex(true, "ShinraMeter", out aIsNewInstance);

            }
            Thread.Sleep(1000);
            var _uniqueUpdating = new Mutex(true, "ShinraMeterUpdating", out isUpdating);
            UpdateManager.DestroyRelease();

            var source = Directory.GetDirectories(UpdateManager.ExecutableDirectory + @"\..\release\")[0];
            UpdateManager.Copy(source, UpdateManager.ExecutableDirectory+@"\..\..\");
            Console.WriteLine("New version installed");
            Process.Start(UpdateManager.ExecutableDirectory + @"\..\..\ShinraMeter.exe");
            Environment.Exit(0);
        }
    }
}
