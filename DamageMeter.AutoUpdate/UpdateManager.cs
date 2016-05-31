using System;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Net;
using System.Net.Http;
using System.Reflection;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace DamageMeter.AutoUpdate
{
    public class UpdateManager
    {
        public static readonly string Version = "1.05";
            
        public static string ExecutableDirectory => Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);

        public static string ResourcesDirectory
        {
            get
            {
                var directory = Path.GetDirectoryName(typeof (UpdateManager).Assembly.Location);
                while (directory != null)
                {
                    var resourceDirectory = Path.Combine(directory, @"resources\");
                    if (Directory.Exists(resourceDirectory))
                        return resourceDirectory;
                    directory = Path.GetDirectoryName(directory);
                }
                throw new InvalidOperationException("Could not find the resource directory");
            }
        }

        public static async Task<bool> Update()
        {
            var isUpToDate = await IsUpToDate().ConfigureAwait(false);
            if (isUpToDate) return false;
            Download();
            return true;
        }

        public static async Task<bool> IsUpToDate()
        {
            var latestVersion = await LatestVersion().ConfigureAwait(false);
            Console.WriteLine("Current version = " + Version);
            Console.WriteLine("Latest version = " + latestVersion);
            return latestVersion == Version;
        }

        private static void Decompress(string latestVersion)
        {
            // Get the stream of the source file.
            ZipFile.ExtractToDirectory(ExecutableDirectory + @"\tmp\" + latestVersion, ExecutableDirectory + @"\tmp\");
            ZipFile.ExtractToDirectory(ExecutableDirectory + @"\tmp\" + latestVersion,
                ExecutableDirectory + @"\tmp\release\");
        }


        private static void Download()
        {
            DestroyDownloadDirectory();
            Directory.CreateDirectory(ExecutableDirectory + @"\tmp\release\");


            var latestVersion = "ShinraMeterV" + LatestVersion().Result;
            Console.WriteLine("Downloading latest version");
            using (var client = new WebClient())
            {
                client.DownloadFile(
                    " http://neowutran.ovh:8083/updates/" + latestVersion +
                    ".zip", ExecutableDirectory + @"\tmp\" + latestVersion + ".zip");
            }
            Console.WriteLine("Latest version downloaded");
            Console.WriteLine("Checksum");
            if (!Checksum(latestVersion, latestVersion + ".zip").Result)
            {
                MessageBox.Show("Invalid checksum, abording upgrade");
                return;
            }
            Console.WriteLine("Decompressing");
            Decompress(latestVersion + ".zip");
            Console.WriteLine("Decompressed");
            Process.Start(ExecutableDirectory + @"\tmp\" + latestVersion + @"\Autoupdate.exe", "pass");
            Console.WriteLine("Start upgrading");
        }

        private static void DestroyDownloadDirectory()
        {
            if (!Directory.Exists(ExecutableDirectory + @"\tmp\")) return;
            Directory.Delete(ExecutableDirectory + @"\tmp\", true);
        }

        public static void Copy(string sourceDir, string targetDir)
        {
            Directory.CreateDirectory(targetDir);
            foreach (var file in Directory.GetFiles(sourceDir))
            {
                File.Copy(file, Path.Combine(targetDir, Path.GetFileName(file)), true);
            }

            foreach (var directory in Directory.GetDirectories(sourceDir))
            {
                if (directory == "config")
                {
                    Directory.CreateDirectory(targetDir);
                    continue;
                }
                Copy(directory, Path.Combine(targetDir, Path.GetFileName(directory)));
            }
        }

        public static void DestroyRelease()
        {
            Array.ForEach(Directory.GetFiles(ExecutableDirectory + @"\..\..\"), File.Delete);
            Array.ForEach(Directory.GetFiles(ExecutableDirectory + @"\..\..\resources\"), File.Delete);
            if (!Directory.Exists(ExecutableDirectory + @"\..\..\resources\")) return;
            var data = ExecutableDirectory + @"\..\..\resources\data\";
            var img = ExecutableDirectory + @"\..\..\resources\img\";
            var ssl = ExecutableDirectory + @"\..\..\resources\ssl\";
            if (Directory.Exists(data))
            {
                Directory.Delete(data, true);
            }
            if (Directory.Exists(img))
            {
                Directory.Delete(img, true);
            }
            if (Directory.Exists(ssl))
            {
                Directory.Delete(ssl, true);
            }

            Console.WriteLine("Resources directory destroyed");
        }


        private static async Task<string> LatestVersion()
        {
            var version =
                await
                    GetResponseText(" http://neowutran.ovh:8083/updates/version.txt")
                        .ConfigureAwait(false);
            version = Regex.Replace(version, @"\r\n?|\n", "");

            return version;
        }

        private static async Task<bool> Checksum(string version, string file)
        {
            var checksum =
                await
                    GetResponseText("http://diclah.com/~yukikoo/" + version + ".txt")
                        .ConfigureAwait(false);
            checksum = Regex.Replace(checksum, @"\r\n?|\n", "");
            string hashString;
            using (var stream = File.OpenRead(ExecutableDirectory + @"\tmp\" + file))
            {
                var sha512 = SHA512.Create();
                var hash = sha512.ComputeHash(stream);
                hashString = BitConverter.ToString(hash);
                hashString = hashString.Replace("-", "");
            }
            checksum = checksum.ToLowerInvariant();
            hashString = hashString.ToLowerInvariant();
            Console.WriteLine("Online checksum:" + checksum);
            Console.WriteLine("Computed checksum:" + hashString);
            return hashString == checksum;
        }

        private static async Task<string> GetResponseText(string address)
        {
            return await GetResponseText(address, 3);
        }

        private static async Task<string> GetResponseText(string address, int numbertry)
        {
            try
            {
                using (var client = new HttpClient())
                {
                    var response = await client.GetByteArrayAsync(new Uri(address));
                    return Encoding.UTF8.GetString(response, 0, response.Length);
                }
            }
            catch (Exception)
            {
                if (numbertry > 0)
                {
                    return await GetResponseText(address, numbertry - 1);
                }
                throw;
            }
        }
    }
}