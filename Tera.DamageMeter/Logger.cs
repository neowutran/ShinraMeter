// Copyright (c) Gothos
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Globalization;
using System.IO;
using System.Linq;

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
            var lines = s.Replace("\r\n", "\r").Replace("\n", "\r").Split('\r');
            var prefix = DateTime.Now.ToString("yyyy-MM-dd HH-mm-ss", CultureInfo.InvariantCulture) + " ";
            var prefix2 = new string(Enumerable.Repeat(' ', prefix.Length).ToArray());
            File.AppendAllLines(LogFile, lines.Select((line, i) => (i == 0 ? prefix : prefix2) + line));
        }
    }
}
