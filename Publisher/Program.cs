using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Security.Cryptography;
using DamageMeter.AutoUpdate;
using SevenZip;
using CompressionLevel = SevenZip.CompressionLevel;
using CompressionMode = SevenZip.CompressionMode;

namespace Publisher
{
    internal class Program
    {
        private static void Main(string[] args)
        {
            var source = SourceDirectory();
            var target = source + UpdateManager.Version;
            if (Directory.Exists(target)) { Directory.Delete(target, true); }
            var libpath = Path.Combine(Path.GetDirectoryName(source), Environment.Is64BitProcess ? "lib/7z_x64.dll" : "lib/7z.dll");
            SevenZipBase.SetLibraryPath(libpath);
            var compressor = new SevenZipCompressor {ArchiveFormat = OutArchiveFormat.Zip};
            compressor.CustomParameters["tc"] = "off";
            compressor.CompressionLevel = CompressionLevel.High;
            compressor.CompressionMode = CompressionMode.Create;
            compressor.TempFolderPath = Path.GetTempPath();
            compressor.PreserveDirectoryRoot = true;
            if (args.Length == 1)
            {
                Console.WriteLine("Unpacking old release");
                Directory.Delete(source + @"\resources", true);
                Array.ForEach(Directory.GetFiles(source, "*", SearchOption.AllDirectories).Where(t => t.EndsWith("zip")).ToArray(),
                    x => ZipFile.ExtractToDirectory(x, Path.GetDirectoryName(x)));
                Array.ForEach(Directory.GetFiles(source, "*", SearchOption.AllDirectories).Where(t => t.EndsWith("zip")).ToArray(), File.Delete);
                return;
            }
            new DirectoryInfo(source).MoveTo(target);
            Console.WriteLine("Compressing main archive");
            compressor.CompressDirectory(target, target + ".zip");
            compressor.PreserveDirectoryRoot = false;
            string hashString;
            using (var stream = File.OpenRead(target + ".zip"))
            {
                var sha512 = SHA512.Create();
                var hash = sha512.ComputeHash(stream);
                hashString = BitConverter.ToString(hash);
                hashString = hashString.Replace("-", "");
            }
            Console.WriteLine("Hashing...");
            File.WriteAllText(target + ".txt", hashString);
            var _hashes = new Dictionary<string, string>();
            Array.ForEach(
                Directory.GetFiles(target, "*", SearchOption.AllDirectories)
                    .Where(t => !t.EndsWith("ShinraLauncher.exe") && !t.Contains(@"\tmp\") && !t.Contains(@"\config\") && !t.Contains(@"\sound\") &&
                                !t.EndsWith("error.log")).ToArray(), x => _hashes.Add(x, UpdateManager.FileHash(x)));
            File.WriteAllLines(source + ".sha1", _hashes.Select(x => x.Value + " *" + x.Key.Replace(target + "\\", "")));
            compressor.CompressFiles(source + ".sha1.zip", source + ".sha1");
            File.Delete(source + ".sha1");
            _hashes.Keys.ToList().ForEach(x =>
            {
                compressor.CompressFiles(x + ".zip", x);
                File.Delete(x);
                Console.WriteLine("Compressing " + x);
            });
            Array.ForEach(Directory.GetFiles(target, "*", SearchOption.AllDirectories).Where(t => !t.EndsWith("zip")).ToArray(), File.Delete);
            new DirectoryInfo(target).MoveTo(source);
        }

        public static string SourceDirectory()
        {
            var directory = Path.GetDirectoryName(typeof(UpdateManager).Assembly.Location);
            Debug.WriteLine(directory);
            while (directory != null)
            {
                var sourceDirectory = Path.Combine(directory, @"ShinraMeterV");
                if (Directory.Exists(sourceDirectory)) { return sourceDirectory; }
                directory = Path.GetDirectoryName(directory);
            }
            throw new InvalidOperationException("Could not find the release directory");
        }
    }
}