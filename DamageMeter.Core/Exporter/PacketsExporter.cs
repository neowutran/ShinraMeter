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
using System.Net;
using System.Net.Http;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using Tera;
using Tera.Game;
using Tera.Game.Messages;
using Tera.PacketLog;

namespace DamageMeter
{
    public class PacketsExporter
    {
        public static PacketsExporter Instance => _instance ?? (_instance = new PacketsExporter());
        private static PacketsExporter _instance;
        private static readonly string PUBLIC_KEY_STRING = "<?xml version=\"1.0\" encoding=\"utf-16\"?><RSAParameters xmlns:xsi=\"http://www.w3.org/2001/XMLSchema-instance\" xmlns:xsd=\"http://www.w3.org/2001/XMLSchema\"><Exponent>AQAB</Exponent><Modulus>sD+HLW7fz2xuQ+JoawSXsZLrb8m7Vn9HVnmkeIJazHDEwPycQrDyYo4XNI27qC2ZhEGlk0qQ1Dd8pFEvhsVVzyve2Ov7CuuuBm7I/rpO1ii9TvEPIjr47eQ5fY4+Trwzjp9au1nw8/E2XNJTFagU1Ch1jJK730BS3ZAbcJSnpUGR0svCnbAc2gpPUJfQxaQgYlr23bdS2dTC/qey/pieg9QhU4N9ZCoYMCshB5+r2wLEfcgHkYtP2aUbUBVGGQ4YtfkX8eIZsRjmMClEzeaVSqvkNh5q5K6qdKFpkc1zZnLKNhwjo/OmcjIc11q/8wlOZPiRKsVe9gC8ySdDCGQXIW9PF2rFYEvTVPWRVeLOPlCfTA1wVXDBlNs5Bchix7pBVumfO2apuizzgWfqm0Q7xyvsHfv7I7ejynjPr5/aEdHzWZK1/RSEwWCSMrstMTzDuuNgOlpYzbAxEpAc1APKAxxjD3C7bgY9IHFNgTpGIYlzJgA6xy2MCWgLm5q0pNjpaiQIBiuCArxMSIn2qpPOkoRLmi2cXHKl27WmjQtBVrw93jRPtLMUSyJ5fsXAVlXy5gnXBl69tQmrvuiRZKWqpZCDhrXHpUEj7J9cULUv0bjzonpAH6UnPVZTIp/VHq+yh0wnbPRUzqcT+ku34U8J3NGYlkf9ZgqGup9EJRka2eE=</Modulus></RSAParameters>";
        public static readonly List<AreaAllowed> BossAllowed = JsonConvert.DeserializeObject<List<AreaAllowed>>(
          "[" +
            "{\"AreaId\":716, \"BossIds\":[1000]}," +
            "{\"AreaId\":769, \"BossIds\":[76903]}," +
            "{\"AreaId\":969, \"BossIds\":[76903]}," +
            "{\"AreaId\": 735,\"BossIds\": [3000]}," +
            "{\"AreaId\": 935,\"BossIds\": [3000]}," +
            "{\"AreaId\": 950,\"BossIds\": [3000, 4000]}," +
            "{\"AreaId\": 794,\"BossIds\": [3000]}," +
            "{\"AreaId\": 994,\"BossIds\": [3000]}," +
            "{\"AreaId\": 916,\"BossIds\": [1000]}" +
            "]"
          );
        public void Export(EncounterBase teradpsData, NpcEntity entity)
        {
            BasicTeraData.LogError("PacketExport: Start", true);
            // Only export when a notable dungeons is cleared
            var areaId = int.Parse(teradpsData.areaId);
            if (!BossAllowed.Any(x => x.AreaId == areaId && (x.BossIds.Count == 0 || x.BossIds.Contains((int)entity.Info.TemplateId)))) {
                BasicTeraData.LogError("PacketExport: Boss not allowed, exiting", true);
                return;
            }
            if (!TeraSniffer.Instance.EnableMessageStorage)
            {
                BasicTeraData.LogError("PacketExport: Option not activated, exiting", true);
                // Message storing have already been stopped
                return;
            }
            // Keep a local reference of the packet list
            Queue<Message> packetsCopyStorage = TeraSniffer.Instance.GetPacketsLogsAndStop();
            if (!packetsCopyStorage.Any())
            {
                BasicTeraData.LogError("PacketExport: Empty packet log, exiting", true);
                return;
            }

            var version = NetworkController.Instance.MessageFactory.Version;
            Guid id = Guid.NewGuid();
            string filename =  version + "_"+ id;

            Debug.WriteLine("Start exporting data");
            BasicTeraData.LogError("PacketExport: Export data to tmp file", true);
            SaveToTmpFile(version.ToString(), packetsCopyStorage, filename+ ".TeraLog");
            BasicTeraData.LogError("PacketExport: Compress file", true);
            Compress(filename + ".TeraLog", filename+".7z");
            File.Delete(filename + ".TeraLog");
            BasicTeraData.LogError("PacketExport: Encrypt file", true);
            Encrypt(filename + ".7z", filename + ".rsa");
            File.Delete(filename + ".7z");
            BasicTeraData.LogError("PacketExport: Send file", true);
            Send(filename + ".rsa", version);
            File.Delete(filename+".rsa");


        }

        private void SaveToTmpFile(string version, Queue<Message> packetsCopyStorage, string filename)
        {
            var header = new LogHeader { Region = version };
            PacketLogWriter writer = new PacketLogWriter(filename, header);
            foreach (var message in packetsCopyStorage)
            {
                ParsedMessage parsedMessage = NetworkController.Instance.MessageFactory.Create(message);
                parsedMessage = WipeoutSensitiveData(parsedMessage);
                writer.Append(message);
            }
            writer.Dispose();
            
        }

        private ParsedMessage WipeoutSensitiveData(ParsedMessage parsedMessage)
        {
            if (parsedMessage.GetType() == typeof(S_CHAT))
            {
                ((S_CHAT)parsedMessage).ReplaceStringWithGarbage(((S_CHAT)parsedMessage).TextOffset);
            }
            else if (parsedMessage.GetType() == typeof(S_WHISPER))
            {
                ((S_WHISPER)parsedMessage).ReplaceStringWithGarbage(((S_WHISPER)parsedMessage).TextOffset);
            }
            else if (parsedMessage.GetType() == typeof(S_PRIVATE_CHAT))
            {
                ((S_PRIVATE_CHAT)parsedMessage).ReplaceStringWithGarbage(((S_PRIVATE_CHAT)parsedMessage).TextOffset);
            }
            else if (parsedMessage.GetType() == typeof(C_CHAT))
            {
                ((C_CHAT)parsedMessage).ReplaceStringWithGarbage(((C_CHAT)parsedMessage).TextOffset);
            }
            else if (parsedMessage.GetType() == typeof(C_WHISPER))
            {
                ((C_WHISPER)parsedMessage).ReplaceStringWithGarbage(((C_WHISPER)parsedMessage).TextOffset);
            }
            return parsedMessage;
        }

        /*
         * Get file byte array
         * Encrypt file byte array using AES and a random key / iv
         * Encrypt the random key/iv using the public RSA key
         * Concat the encrypted key/iv + encrypted file to a new file
         */
        private void Encrypt(string inputFilename, string outputFilename)
        {
            var sr = new StringReader(PUBLIC_KEY_STRING);
            //we need a deserializer
            var xs = new System.Xml.Serialization.XmlSerializer(typeof(RSAParameters));
            //get the object back from the stream
            var publicKey = (RSAParameters)xs.Deserialize(sr);
            
            var csp = new RSACryptoServiceProvider();
            csp.ImportParameters(publicKey);
            var clearData = File.ReadAllBytes(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, inputFilename));

            Aes aes = Aes.Create();
            aes.KeySize = 256;
            aes.Mode = CipherMode.CBC;
            aes.BlockSize = 128;
            aes.GenerateIV();
            aes.GenerateKey();

            using (var fs = new FileStream(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, outputFilename), FileMode.Create, FileAccess.Write))
            {
                var encryptedAESKey = csp.Encrypt(aes.Key, true);
                var encryptedAESIV = csp.Encrypt(aes.IV, true);
                var fileheaderString = BitConverter.ToString(encryptedAESKey).Replace("-", string.Empty) +
                    "</EncryptedAESKey>" +
                    BitConverter.ToString(encryptedAESIV).Replace("-", string.Empty) +
                    "</EncryptedAESIV>";
                var fileheaderBytes = Encoding.ASCII.GetBytes(fileheaderString);
                fs.Write(fileheaderBytes, 0, fileheaderBytes.Length);

                using (var encryptor = aes.CreateEncryptor())
                using (var encrypt = new CryptoStream(fs, encryptor, CryptoStreamMode.Write))
                {
                    encrypt.Write(clearData, 0, clearData.Length);
                    encrypt.FlushFinalBlock();
                }             
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
            compressor.CompressFiles(outputFilename, new string[]{ Path.Combine(AppDomain.CurrentDomain.BaseDirectory,inputFilename)});
        }

        private void Send(string filename, uint version)
        {
            var filebytes = File.ReadAllBytes(filename);
            
            SHA1Managed sha = new SHA1Managed();
            byte[] checksum = sha.ComputeHash(filebytes);
            var sendCheckSum = BitConverter.ToString(checksum).Replace("-", string.Empty);
            Debug.WriteLine(sendCheckSum);
            BasicTeraData.LogError("PacketExport: Send hash: "+sendCheckSum, true);
            
            using (var client = new HttpClient())
            {
                client.Timeout = TimeSpan.FromSeconds(3600);
                var response = client.PostAsync(
                    new Uri("https://neowutran.ovh/storage/store_packets.php?version=" + version + "&sha1=" + sendCheckSum),
                    new ByteArrayContent(filebytes)
                    );
                BasicTeraData.LogError("PacketExport: "+ response.Result.Content.ReadAsStringAsync().Result, true);
                Debug.WriteLine(response.Result.Content.ReadAsStringAsync().Result);
            }
        }
    }
}
