using System;
using System.Diagnostics;
using System.IO;
using System.Net.Http;
using System.Threading;
using System.Windows.Forms;

namespace DamageMeter.AutoUpdate
{
    public class Program
    {
        private static void Main(string[] args)
        {
            if (args.Length == 0)
            {
                Process.Start( new ProcessStartInfo("https://www.youtube.com/watch?v=dQw4w9WgXcQ") { UseShellExecute = true});
                MessageBox.Show("Autoupdate.exe is used internally by the meter. End user must not run it.");
                Environment.Exit(0);
            }

            bool aIsNewInstance, isUpdating;
            var _unique = new Mutex(true, "ShinraMeter", out aIsNewInstance);
            if (!aIsNewInstance)
            {
                try { while (!_unique.WaitOne(1000)) { Console.WriteLine("Sleep"); } }
                catch (AbandonedMutexException) { } //ignore terminated meter
            }
            Thread.Sleep(1000);
            var uniqueUpdating = new Mutex(true, "ShinraMeterUpdating", out isUpdating);
            var hashfile = UpdateManager.ExecutableDirectory + @"\ShinraMeterV.sha1";
            if (File.Exists(hashfile))
            {
                var hashes = UpdateManager.ReadHashFile(hashfile, UpdateManager.ExecutableDirectory + @"\..\");
                UpdateManager.CleanupRelease(hashes);
                UpdateManager.Copy(UpdateManager.ExecutableDirectory + @"\release\", UpdateManager.ExecutableDirectory + @"\..\");
                UpdateManager.ReadDbVersion();
                CountError(0);
                Console.WriteLine("New version installed");
                //Process.Start("explorer.exe", "https://github.com/neowutran/ShinraMeter/wiki/Patch-note");
                Process.Start(UpdateManager.ExecutableDirectory + @"\..\ShinraMeter.exe");
            }
            else
            {
                UpdateManager.DestroyRelease();
                CountError(0);
                var source = Directory.GetDirectories(UpdateManager.ExecutableDirectory + @"\..\release\")[0];
                UpdateManager.Copy(source, UpdateManager.ExecutableDirectory + @"\..\..\");
                Console.WriteLine("New version installed");
                Process.Start("explorer.exe", "https://github.com/neowutran/ShinraMeter/wiki/Patch-note");
                Process.Start(UpdateManager.ExecutableDirectory + @"\..\..\ShinraMeter.exe");
            }
            uniqueUpdating.ReleaseMutex();
            _unique.ReleaseMutex();
            Environment.Exit(0);
        }

        private static void CountError(int numberTry)
        {
            if (numberTry > 2)
            {
                Console.WriteLine("Error");
                return;
            }
            try
            {
                if (Count()) { return; }
                numberTry++;
                CountError(numberTry);
            }
            catch
            {
                numberTry++;
                CountError(numberTry);
            }
        }

        private static bool Count()
        {
            using (var client = new HttpClient())
            {
                var response = client.GetAsync(new Uri("http://diclah.com/~yukikoo/counter/counter.php?version=" + UpdateManager.Version)).Result;
                return response.IsSuccessStatusCode;
            }
        }
    }
}