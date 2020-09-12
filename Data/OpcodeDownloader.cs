using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using Newtonsoft.Json;

namespace Data
{
    public class OpcodeDownloader
    {
        public static void DownloadIfNotExist(uint version, String directory)
        {
            var res = DownloadOpcode(version, directory);
            if (res) BasicTeraData.LogError("Updated opcodes: " + version);
        }

        private static bool DownloadOpcode(uint version, String directory)
        {
            var filename = directory + Path.DirectorySeparatorChar + version + ".txt";
            if (File.Exists(filename))
            {
                return false;
            }
            filename = directory + Path.DirectorySeparatorChar + "protocol." + version + ".map";
            if (File.Exists(filename))
            {
                return false;
            }
            try
            {
                Download("https://raw.githubusercontent.com/neowutran/TeraDpsMeterData/master/opcodes/protocol." + version + ".map", filename);
                return true;
            }
            catch { }
            try
            {
                ToolboxOpcodes("https://raw.githubusercontent.com/tera-toolbox/tera-toolbox/master/data/data.json", directory);
                if (File.Exists(filename)) return true;
            }
            catch { }
            try
            {
                ToolboxOpcodes("https://raw.githubusercontent.com/tera-toolbox/tera-toolbox/beta/data/data.json", directory);
                if (File.Exists(filename)) return true;
            }
            catch { }
            try
            {
                Download("https://raw.githubusercontent.com/tera-proxy/tera-data/master/map/protocol." + version + ".map", filename);
                return true;
            }
            catch { }
            return false;
        }

        public static bool DownloadSysmsg(uint version, int revision, String directory)
        {
            var filename = directory + Path.DirectorySeparatorChar + "smt_" + version + ".txt";
            if (File.Exists(filename))
            {
                return false;
            }
            filename = directory + Path.DirectorySeparatorChar + "sysmsg." + version + ".map";
            if (File.Exists(filename))
            {
                return false;
            }
            filename = directory + Path.DirectorySeparatorChar + "sysmsg." + revision/100 + ".map";
            if (File.Exists(filename))
            {
                return false;
            }
            try
            {
                Download("https://raw.githubusercontent.com/neowutran/TeraDpsMeterData/master/opcodes/sysmsg." + version + ".map", "sysmsg." + version + ".map");
                return true;
            }
            catch { }
            try
            {
                Download("https://raw.githubusercontent.com/neowutran/TeraDpsMeterData/master/opcodes/sysmsg." + revision/100 + ".map", filename);
                return true;
            }
            catch { }
            try
            {
                Download("https://raw.githubusercontent.com/tera-proxy/tera-data/master/map/sysmsg." + revision / 100 + ".map", filename);
                return true;
            }
            catch { }
            return false;
        }

        private static void Download(String remote, String local)
        {
            using (var client = new WebClient())
            {
                client.DownloadFile(remote, local);
            }
        }

        public static void ToolboxOpcodes(string url, string directory)
        {
            using var client = new WebClient();
            var json = client.DownloadString(url);
            var parsed = JsonConvert.DeserializeObject<ToolboxTeraData>(json);
            foreach (var map in parsed.maps)
            {
                var fname = Path.Combine(directory, $"protocol.{map.Key}.map");
                if (!File.Exists(fname)) File.WriteAllText(fname, string.Join("\n", map.Value.Select(x => x.Key + " " + x.Value)));
            }
        }
        public class ToolboxTeraData
        {
            public Dictionary<string, Dictionary<string, int>> maps { get; set; }
            //public Dictionary<string, string> protocol { get; set; }
            //public dynamic deprecated { get; set; }
        }

    }
}
