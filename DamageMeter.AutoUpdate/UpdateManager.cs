using System;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Net;
using System.Net.Http;
using System.Reflection;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace DamageMeter.AutoUpdate
{
    public class UpdateManager
    {
        public static readonly string Version = "0.44";

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
            SetCertificate();
            using (var client = new WebClient())
            {
                client.DownloadFile(
                    "https://cloud.neowutran.ovh/index.php/s/e7arRRxkHEIkzU1/download?path=%2F&files=" + latestVersion +
                    ".zip", ExecutableDirectory + @"\tmp\" + latestVersion + ".zip");
            }
            Console.WriteLine("Latest version downloaded");
            Console.WriteLine("Checksum");
            if (!(Checksum(latestVersion, latestVersion + ".zip").Result))
            {
                MessageBox.Show("Invalid checksum, abording upgrade");
                return;
            }
            Console.WriteLine("Decompressing");
            Decompress(latestVersion + ".zip");
            Console.WriteLine("Decompressed");
            Process.Start("Explorer.exe", ExecutableDirectory + @"\tmp\" + latestVersion + @"\Autoupdate.exe");
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
                Copy(directory, Path.Combine(targetDir, Path.GetFileName(directory)));
            }
        }

        public static void DestroyRelease()
        {
            if (!Directory.Exists(ExecutableDirectory + @"\..\..\resources\")) return;
            Directory.Delete(ExecutableDirectory + @"\..\..\resources\", true);
            Console.WriteLine("Resources directory destroyed");
        }


        private static async Task<string> LatestVersion()
        {
            var version =
                await
                    GetResponseText("https://cloud.neowutran.ovh/index.php/s/muOLoJjP8JJfqFR/download")
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

        private static void SetCertificate()
        {
            var cloudCertificate = new X509Certificate2(ResourcesDirectory + @"cloud.neowutran.ovh.der");
            ServicePointManager.ServerCertificateValidationCallback =
                (sender, certificate, chain, sslPolicyErrors) =>
                    certificate.Equals(cloudCertificate);
        }

        private static async Task<string> GetResponseText(string address)
        {
            SetCertificate();
            using (var client = new HttpClient())
            {
                Console.WriteLine(address);
                return await client.GetStringAsync(new Uri(address));
            }
        }
    }
}