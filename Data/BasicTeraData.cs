using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Management;
using System.Net.Http;
using System.Threading.Tasks;
using DamageMeter.AutoUpdate;
using log4net;
using log4net.Config;
using Lang;
using Tera.Game;

namespace Data
{
    public class BasicTeraData
    {
        private static BasicTeraData _instance;
        private static readonly ILog Log = LogManager.GetLogger("ShinraMeter");
        private static int _errorCount = 10; //limit number of debug messages in one session
        private static string _region = "Unknown";
        private readonly Func<string, TeraData> _dataForRegion;

       
        
        private BasicTeraData() : this(FindResourceDirectory()) { }

        private BasicTeraData(string resourceDirectory)
        {
            ResourceDirectory = resourceDirectory;
            Directory.CreateDirectory(Path.Combine(resourceDirectory, "config")); //ensure config dir is created
            XmlConfigurator.Configure(new Uri(Path.Combine(ResourceDirectory, "log4net.xml")));
            HotkeysData = new HotkeysData(this);
            WindowData = new WindowData(this);
            LP.Culture = WindowData.UILanguage != "Auto" ? CultureInfo.GetCultureInfo(WindowData.UILanguage) : CultureInfo.CurrentUICulture;
            EventsData = new EventsData(this);
            _dataForRegion = Helpers.Memoize<string, TeraData>(region => new TeraData(region));
            Servers = new ServerDatabase(Path.Combine(ResourceDirectory, "data"));
            //handle overrides
            var serversOverridePath = Path.Combine(ResourceDirectory, "config/server-overrides.txt");
            if (!File.Exists(serversOverridePath)) //create the default file if it doesn't exist
            {
                File.WriteAllText(serversOverridePath, LP.ServerOverrides);
            }
            var overriddenServers = GetServers(serversOverridePath).ToList();
            Servers.AddOverrides(overriddenServers);


            ImageDatabase = new ImageDatabase(Path.Combine(ResourceDirectory, "img/"));
            Icons = new IconsDatabase(Path.Combine(ResourceDirectory, "data/"));
            
            // change later 
            ;
        }


        //public QuestInfoDatabase QuestInfoDatabase { get; set; }
        public HotDotDatabase HotDotDatabase { get; set; }
        public static BasicTeraData Instance => _instance ?? (_instance = new BasicTeraData());
        public PetSkillDatabase PetSkillDatabase { get; set; }
        public SkillDatabase SkillDatabase { get; set; }
        public ImageDatabase ImageDatabase { get; }
        public NpcDatabase MonsterDatabase { get; set; }
        public WindowData WindowData { get; }
        public EventsData EventsData { get; }
        public HotkeysData HotkeysData { get; }
        public string ResourceDirectory { get; }
        public ServerDatabase Servers { get; }
        public IconsDatabase Icons { get; set; }
        public MapData MapData { get; set; }

        private static IEnumerable<Server> GetServers(string filename)
        {
            return File.ReadAllLines(filename).Where(s => !s.StartsWith("#") && !string.IsNullOrWhiteSpace(s)).Select(s => s.Split(new[] {' '}, 3))
                .Select(parts => new Server(parts[2], parts[1], parts[0]));
        }

        public TeraData DataForRegion(string region)
        {
            _region = region;
            return _dataForRegion(region);
        }

        private static string FindResourceDirectory()
        {
            var directory = Path.GetDirectoryName(typeof(BasicTeraData).Assembly.Location);
            while (directory != null)
            {
                var resourceDirectory = Path.Combine(directory, @"resources\");
                if (Directory.Exists(resourceDirectory)) { return resourceDirectory; }
                directory = Path.GetDirectoryName(directory);
            }
            throw new InvalidOperationException("Could not find the resource directory");
        }

        public static void LogError(string error, bool local = false, bool debug = false)
        {
            if (debug && _errorCount-- <= 0) { return; }
            Log.Error(error);
            Task.Run(() =>
            {
                try
                {
                    if (!Instance.WindowData.Debug || local) { return; }
                    var name = (from x in new ManagementObjectSearcher("SELECT * FROM Win32_OperatingSystem").Get().Cast<ManagementObject>()
                                   select x.GetPropertyValue("Version") + " Memory Total:" + x.GetPropertyValue("TotalVisibleMemorySize") + " Virtual:" +
                                          x.GetPropertyValue("TotalVirtualMemorySize") + " PhFree:" + x.GetPropertyValue("FreePhysicalMemory") + " VFree:" +
                                          x.GetPropertyValue("FreeVirtualMemory")).FirstOrDefault() ?? "unknown";
                    name = name + " CPU:" + ((from x in new ManagementObjectSearcher("SELECT * FROM Win32_Processor").Get().Cast<ManagementObject>()
                                                 select x.GetPropertyValue("Name") + " load:" + x.GetPropertyValue("LoadPercentage") + "%").FirstOrDefault() ??
                                             "processor unknown");
                    error = $"##### (version={UpdateManager.Version} Region={_region}) running on {name}:\r\n" + (debug ? "##### Debug: " : "") + error;

                    using (var client = new HttpClient())
                    {
                        var formContent = new FormUrlEncodedContent(new[] {new KeyValuePair<string, string>("error", error)});

                        var response = client.PostAsync("http://diclah.com/~yukikoo/debug/debug.php", formContent);
                        var responseString = response.Result.Content.ReadAsStringAsync();
                        Console.WriteLine(responseString.Result);
                    }
                }
                catch
                {
                    // Ignore
                }
            });
        }
    }
}