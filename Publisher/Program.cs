using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using DamageMeter.AutoUpdate;
using SevenZip;
using CompressionLevel = System.IO.Compression.CompressionLevel;

namespace Publisher
{
    class Program
    {
        static void Main(string[] args)
        {
            var source = SourceDirectory();
            var target = source + UpdateManager.Version;
            if (Directory.Exists(target))
            {
                Directory.Delete(target, true);
            }
            string libpath;
            if (Environment.Is64BitProcess)
                libpath = Path.Combine(Path.GetDirectoryName(source), "lib/7z_x64.dll");
            else
                libpath = Path.Combine(Path.GetDirectoryName(source), "lib/7z.dll");
            SevenZipBase.SetLibraryPath(libpath);
            new DirectoryInfo(source).MoveTo(target);
            var compressor = new SevenZipCompressor();
            compressor.ArchiveFormat=OutArchiveFormat.Zip;
            compressor.CompressionLevel=SevenZip.CompressionLevel.High;
            compressor.CompressionMode = CompressionMode.Create;
            compressor.TempFolderPath = Path.GetTempPath();
            compressor.PreserveDirectoryRoot = true;
            compressor.CompressDirectory(target, target+".zip", true);
            compressor.PreserveDirectoryRoot = false;
            string hashString;
            using (var stream = File.OpenRead(target + ".zip"))
            {
                var sha512 = SHA512.Create();
                var hash = sha512.ComputeHash(stream);
                hashString = BitConverter.ToString(hash);
                hashString = hashString.Replace("-", "");
            }
            File.WriteAllText(target+".txt", hashString);
            var _hashes = new Dictionary<string, string>();
            Array.ForEach(Directory.GetFiles(target, "*", SearchOption.AllDirectories).Where(t =>
                  !t.EndsWith("ShinraLauncher.exe") && !t.Contains("tmp") && !t.Contains("config") && !t.Contains("sound") && !t.EndsWith("error.log")
                    ).ToArray(), x => _hashes.Add(x, UpdateManager.FileHash(x)));
            File.WriteAllLines(target+".sha1",_hashes.Select(x=>x.Value+" *"+x.Key.Replace(target + "\\", "")));
            _hashes.Keys.ToList().ForEach(x=> { compressor.CompressFiles(x + ".zip", x); File.Delete(x); });
            Array.ForEach(Directory.GetFiles(target, "*", SearchOption.AllDirectories).Where(t => !t.EndsWith("zip")).ToArray(), File.Delete);
            new DirectoryInfo(target).MoveTo(source);
        }

        public static string SourceDirectory()
        {
            var directory = Path.GetDirectoryName(typeof(UpdateManager).Assembly.Location);
            while (directory != null)
            {
                var sourceDirectory = Path.Combine(directory, @"ShinraMeterV");
                if (Directory.Exists(sourceDirectory))
                    return sourceDirectory;
                directory = Path.GetDirectoryName(directory);
            }
            throw new InvalidOperationException("Could not find the release directory");
        }
    }
}
