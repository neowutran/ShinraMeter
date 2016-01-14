using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using log4net.Config;
using Tera.Game;

namespace Data
{
    public class BasicTeraData
    {
        private static BasicTeraData _instance;
        private readonly Func<string, TeraData> _dataForRegion;

        private BasicTeraData() : this(FindResourceDirectory())
        {
        }

        private BasicTeraData(string resourceDirectory)
        {
            ResourceDirectory = resourceDirectory;
            XmlConfigurator.Configure(new Uri(Path.Combine(ResourceDirectory, "log4net.xml")));
            HotkeysData = new HotkeysData(this);
            WindowData = new WindowData(this);
            _dataForRegion = Memoize<string, TeraData>(region => new TeraData(this, region));
            Servers = GetServers(Path.Combine(ResourceDirectory, "data/servers.txt")).ToList();
            MonsterDatabase = new MonsterDatabase(Path.Combine(ResourceDirectory, "data/monsters/"), Path.Combine(ResourceDirectory, "data/"), WindowData.Language);
            PinData = new PinData(Path.Combine(ResourceDirectory, "img/"));
            PetSkillDatabase = new PetSkillDatabase(Path.Combine(ResourceDirectory, "data/"), WindowData.Language);
            SkillDatabase = new SkillDatabase(Path.Combine(ResourceDirectory, "data/skills/"), Path.Combine(ResourceDirectory, "data/"), WindowData.Language);
            HotDotDatabase = new HotDotDatabase(Path.Combine(ResourceDirectory, "data/hotdot/"), WindowData.Language);
        }
        
        public HotDotDatabase HotDotDatabase { get; }
        public static BasicTeraData Instance => _instance ?? (_instance = new BasicTeraData());
        public PetSkillDatabase PetSkillDatabase { get; private set; }
        public SkillDatabase SkillDatabase { get; private set; }
        public PinData PinData { get; private set; }
        public MonsterDatabase MonsterDatabase { get; private set; }
        public WindowData WindowData { get; }
        public HotkeysData HotkeysData { get; private set; }
        public string ResourceDirectory { get; }
        public IEnumerable<Server> Servers { get; private set; }

        private static Func<T, TResult> Memoize<T, TResult>(Func<T, TResult> func)
        {
            var lookup = new ConcurrentDictionary<T, TResult>();
            return x => lookup.GetOrAdd(x, func);
        }

        public TeraData DataForRegion(string region)
        {
            return _dataForRegion(region);
        }

        private static string FindResourceDirectory()
        {
            var directory = Path.GetDirectoryName(typeof (BasicTeraData).Assembly.Location);
            while (directory != null)
            {
                var resourceDirectory = Path.Combine(directory, @"resources\");
                if (Directory.Exists(resourceDirectory))
                    return resourceDirectory;
                directory = Path.GetDirectoryName(directory);
            }
            throw new InvalidOperationException("Could not find the resource directory");
        }

        private static IEnumerable<Server> GetServers(string filename)
        {
            return File.ReadAllLines(filename)
                .Where(s => !string.IsNullOrWhiteSpace(s))
                .Select(s => s.Split(new[] {' '}, 3))
                .Select(parts => new Server(parts[2], parts[1], parts[0]));
        }
    }
}