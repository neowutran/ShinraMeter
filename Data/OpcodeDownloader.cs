using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

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
            String filename = directory + Path.DirectorySeparatorChar + version + ".txt";
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
                Download("https://raw.githubusercontent.com/caali-hackerman/tera-data/master/map_base/protocol." + version + ".map", filename);
                return true;
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
            String filename = directory + Path.DirectorySeparatorChar + "smt_" + version + ".txt";
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
                Download("https://raw.githubusercontent.com/caali-hackerman/tera-data/master/map_base/sysmsg." + version + ".map", "sysmsg." + version + ".map");
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
            using (WebClient client = new WebClient())
            {
                client.DownloadFile(remote, local);
            }
        }
    }
}
