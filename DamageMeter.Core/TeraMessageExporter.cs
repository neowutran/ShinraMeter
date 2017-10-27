using DamageMeter.Sniffing;
using DamageMeter.TeraDpsApi;
using Data;
using Newtonsoft.Json;
using SevenZip;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Tera;
using Tera.Game;
using Tera.PacketLog;

namespace DamageMeter
{
    public class TeraMessageExporter
    {
        public static TeraMessageExporter Instance => _instance ?? (_instance = new TeraMessageExporter());
        private static TeraMessageExporter _instance;
        private static readonly string PUBLIC_KEY_STRING = "<?xml version=\"1.0\" encoding=\"utf-16\"?><RSAParameters xmlns:xsi=\"http://www.w3.org/2001/XMLSchema-instance\" xmlns:xsd=\"http://www.w3.org/2001/XMLSchema\"><Exponent>AQAB</Exponent><Modulus>sD+HLW7fz2xuQ+JoawSXsZLrb8m7Vn9HVnmkeIJazHDEwPycQrDyYo4XNI27qC2ZhEGlk0qQ1Dd8pFEvhsVVzyve2Ov7CuuuBm7I/rpO1ii9TvEPIjr47eQ5fY4+Trwzjp9au1nw8/E2XNJTFagU1Ch1jJK730BS3ZAbcJSnpUGR0svCnbAc2gpPUJfQxaQgYlr23bdS2dTC/qey/pieg9QhU4N9ZCoYMCshB5+r2wLEfcgHkYtP2aUbUBVGGQ4YtfkX8eIZsRjmMClEzeaVSqvkNh5q5K6qdKFpkc1zZnLKNhwjo/OmcjIc11q/8wlOZPiRKsVe9gC8ySdDCGQXIW9PF2rFYEvTVPWRVeLOPlCfTA1wVXDBlNs5Bchix7pBVumfO2apuizzgWfqm0Q7xyvsHfv7I7ejynjPr5/aEdHzWZK1/RSEwWCSMrstMTzDuuNgOlpYzbAxEpAc1APKAxxjD3C7bgY9IHFNgTpGIYlzJgA6xy2MCWgLm5q0pNjpaiQIBiuCArxMSIn2qpPOkoRLmi2cXHKl27WmjQtBVrw93jRPtLMUSyJ5fsXAVlXy5gnXBl69tQmrvuiRZKWqpZCDhrXHpUEj7J9cULUv0bjzonpAH6UnPVZTIp/VHq+yh0wnbPRUzqcT+ku34U8J3NGYlkf9ZgqGup9EJRka2eE=</Modulus></RSAParameters>";

        public void Export(EncounterBase teradpsData, NpcEntity entity)
        {
            // Only export when a notable dungeons is cleared
            var areaId = int.Parse(teradpsData.areaId);
            if (!DpsServer.DefaultAreaAllowed.Any(x => x.AreaId == areaId && (x.BossIds.Count == 0 || x.BossIds.Contains((int)entity.Info.TemplateId)))) { return; }

            // Keep a local reference of the packet list
            ConcurrentQueue<Message> packetsCopyStorage = TeraSniffer.Instance.PacketsCopyStorage;

            // Stop filling the packet list & delete the original reference, so memory will be freed 
            // by the garbage collector after the export
            TeraSniffer.Instance.EnableMessageStorage = false;
            var version = NetworkController.Instance.MessageFactory.Version;
            Guid id = Guid.NewGuid();
            string filename = id + "_" + version;

            Debug.WriteLine("Start exporting data");
            SaveToTmpFile(version.ToString(), packetsCopyStorage, filename+ ".TeraLog");
            Compress(filename + ".TeraLog", filename+".7z");
            File.Delete(filename + ".TeraLog");
            Encrypt(filename + ".7z", filename + ".rsa");
            File.Delete(filename + ".7z");
            //Send(filename + ".rsa");
            //File.Delete(filename+".rsa");

        }   

        private void SaveToTmpFile(string version, ConcurrentQueue<Message> packetsCopyStorage, string filename)
        {
            var header = new LogHeader { Region = version };
            PacketLogWriter writer = new PacketLogWriter(filename, header);
            foreach (var message in packetsCopyStorage)
            {
                writer.Append(message);
            }
            writer.Dispose();
            
        }


        private void Encrypt(string inputFilename, string outputFilename)
        {
            var sr = new StringReader(PUBLIC_KEY_STRING);
            //we need a deserializer
            var xs = new System.Xml.Serialization.XmlSerializer(typeof(RSAParameters));
            //get the object back from the stream
            var publicKey = (RSAParameters)xs.Deserialize(sr);

            var csp = new RSACryptoServiceProvider();
            csp.ImportParameters(publicKey);

            var clearData = File.ReadAllBytes(inputFilename);
            var encryptedData = csp.Encrypt(clearData, false);

            using (var fs = new FileStream(outputFilename, FileMode.Create, FileAccess.Write))
            {
                fs.Write(encryptedData, 0, encryptedData.Length);
            }

        }

        private void Compress(string inputFilename, string outputFilename)
        {
            var libpath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, Environment.Is64BitProcess ? "lib/7z_x64.dll" : "lib/7z.dll");
            SevenZipBase.SetLibraryPath(libpath);
            var compressor = new SevenZipCompressor { ArchiveFormat = OutArchiveFormat.SevenZip };
            compressor.CustomParameters["tc"] = "off";
            compressor.CompressionLevel = CompressionLevel.Ultra;
            compressor.CompressionMode = CompressionMode.Create;
            compressor.TempFolderPath = Path.GetTempPath();
            compressor.PreserveDirectoryRoot = false;
            compressor.CompressFiles(outputFilename, new string[]{ inputFilename});
        }

        private void Send(string filename)
        {
            // TODO
        }

    }
}
