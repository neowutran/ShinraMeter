using System;
using System.Diagnostics;
using System.IO;
using System.Threading;

namespace DamageMeter.AutoUpdate
{
    public class Program
    {
        private static void Main()
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
            var uniqueUpdating = new Mutex(true, "ShinraMeterUpdating", out isUpdating);
            UpdateManager.DestroyRelease();

            var source = Directory.GetDirectories(UpdateManager.ExecutableDirectory + @"\..\release\")[0];
            UpdateManager.Copy(source, UpdateManager.ExecutableDirectory + @"\..\..\");
            Console.WriteLine("New version installed");
            Process.Start(UpdateManager.ExecutableDirectory + @"\..\..\ShinraMeter.exe");
            Environment.Exit(0);
        }
    }
}