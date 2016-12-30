using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using log4net;
using log4net.Config;
using Tera.Game;
using DamageMeter.AutoUpdate;
using Lang;

namespace Data
{
    public class BasicTeraData
    {
        private static BasicTeraData _instance;
        private readonly Func<string, TeraData> _dataForRegion;
        private static readonly ILog _log = LogManager.GetLogger("ShinraMeter");
        private static int _errorCount = 10; //limit number of debug messages in one session

        private BasicTeraData() : this(FindResourceDirectory())
        {
        }

        private BasicTeraData(string resourceDirectory)
        {
            ResourceDirectory = resourceDirectory;
            Directory.CreateDirectory(Path.Combine(resourceDirectory, "config")); //ensure config dir is created
            XmlConfigurator.Configure(new Uri(Path.Combine(ResourceDirectory, "log4net.xml")));
            HotkeysData = new HotkeysData(this);
            WindowData = new WindowData(this);
            EventsData = new EventsData(this);
            _dataForRegion = Helpers.Memoize<string, TeraData>(region => new TeraData(region));
            Servers = new ServerDatabase(Path.Combine(ResourceDirectory, "data"));
            //handle overrides
            var serversOverridePath = Path.Combine(ResourceDirectory, "config/server-overrides.txt");
            if (!File.Exists(serversOverridePath))//create the default file if it doesn't exist
                File.WriteAllText(serversOverridePath, LP.ServerOverrides );
            var overriddenServers = GetServers(serversOverridePath).ToList();
            Servers.AddOverrides(overriddenServers);


            ImageDatabase = new ImageDatabase(Path.Combine(ResourceDirectory, "img/"));
            Icons = new IconsDatabase(Path.Combine(ResourceDirectory, "data/"));
        }

        private static IEnumerable<Server> GetServers(string filename)
        {
            return File.ReadAllLines(filename)
                       .Where(s => !s.StartsWith("#") && !string.IsNullOrWhiteSpace(s))
                       .Select(s => s.Split(new[] { ' ' }, 3))
                       .Select(parts => new Server(parts[2], parts[1], parts[0]));
        }



        public QuestInfoDatabase QuestInfoDatabase { get; set; }
        public HotDotDatabase HotDotDatabase { get; set; }
        public static BasicTeraData Instance => _instance ?? (_instance = new BasicTeraData());
        public PetSkillDatabase PetSkillDatabase { get; set; }
        public SkillDatabase SkillDatabase { get; set; }
        public ImageDatabase ImageDatabase { get; private set; }
        public NpcDatabase MonsterDatabase { get; set; }
        public WindowData WindowData { get; }
        public EventsData EventsData { get; }
        public HotkeysData HotkeysData { get; private set; }
        public string ResourceDirectory { get; }
        public ServerDatabase Servers { get; private set; }
        public IconsDatabase Icons { get; set; }

        public TeraData DataForRegion(string region)
        {
            return _dataForRegion(region);
        }

        private static string FindResourceDirectory()
        {
            var directory = Path.GetDirectoryName(typeof(BasicTeraData).Assembly.Location);
            while (directory != null)
            {
                var resourceDirectory = Path.Combine(directory, @"resources\");
                if (Directory.Exists(resourceDirectory))
                    return resourceDirectory;
                directory = Path.GetDirectoryName(directory);
            }
            throw new InvalidOperationException("Could not find the resource directory");
        }

            public static void LogError(string error, bool local = false, bool debug = false)
        {
            if (debug && _errorCount-- <= 0) return;
            Task.Run(() =>
            {
                try
                {
                    error = $"##### (version={UpdateManager.Version}):\r\n" + (debug?"##### Debug: ":"") + error;
                    _log.Error(error);
                    if (!Instance.WindowData.Debug || local)
                    {
                        return;
                    }

                    using (var client = new HttpClient())
                    {
                        var formContent = new FormUrlEncodedContent(new[]
                        {
                            new KeyValuePair<string, string>("error", error)
                        });

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