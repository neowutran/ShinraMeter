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
            DownloadOpcode(version, directory);
            DownloadSysmsg(version, directory);
        }

        private static void DownloadOpcode(uint version, String directory)
        {
            String filename = directory + Path.DirectorySeparatorChar + version + ".txt";
            if (File.Exists(filename))
            {
                return;
            }
            try
            {
                Download("https://raw.githubusercontent.com/neowutran/TeraDpsMeterData/master/opcodes/" + version + ".txt", filename);
                return;
            }
            catch { }
        }

        private static void DownloadSysmsg(uint version, String directory)
        {
            String filename = directory + Path.DirectorySeparatorChar + "smt_" + version + ".txt";
            if (File.Exists(filename))
            {
                return;
            }
            try
            {
                Download("https://raw.githubusercontent.com/neowutran/TeraDpsMeterData/master/opcodes/smt_" + version + ".txt", filename);
                return;
            }
            catch { }
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
