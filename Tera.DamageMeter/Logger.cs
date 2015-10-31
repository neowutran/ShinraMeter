using System;
using System.Globalization;
using System.IO;

namespace Tera.DamageMeter
{
    class Logger
    {
        public static readonly string LogDirectory = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "GothosTeraDamageMeter");
        public static readonly string LogFile = Path.Combine(LogDirectory, "Log.txt");

        public static void Clear()
        {
            if (File.Exists(LogFile))
                File.Delete(LogFile);
        }

        public static void Log(string s)
        {
            Directory.CreateDirectory(LogDirectory);
            File.AppendAllLines(LogFile, new[] { DateTime.Now.ToString("yyyy-MM-dd HH-mm-ss", CultureInfo.InvariantCulture) + " " + s });
        }
    }
}
