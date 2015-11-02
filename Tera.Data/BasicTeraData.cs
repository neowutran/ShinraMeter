using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Tera.Game;

namespace Tera.Data
{
    public class BasicTeraData
    {
        private readonly Func<string, TeraData> _dataForRegion;

        public BasicTeraData()
            : this(FindResourceDirectory())
        {
        }

        public BasicTeraData(string resourceDirectory)
        {
            ResourceDirectory = resourceDirectory;
            HotkeysData = new HotkeysData(this);
            WindowData = new WindowData(this);
            _dataForRegion = Memoize<string, TeraData>(region => new TeraData(this, region));
            Servers = GetServers(Path.Combine(ResourceDirectory, "servers.txt")).ToList();
            Regions = GetRegions(Path.Combine(ResourceDirectory, "regions.txt")).ToList();
        }

        public WindowData WindowData { get; }
        public HotkeysData HotkeysData { get; }
        public string ResourceDirectory { get; }
        public IEnumerable<Server> Servers { get; private set; }
        public IEnumerable<Region> Regions { get; private set; }

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

        private static IEnumerable<Region> GetRegions(string filename)
        {
            return File.ReadAllLines(filename)
                .Where(s => !string.IsNullOrWhiteSpace(s))
                .Select(s => s.Split(' '))
                .Select(parts => new Region(parts[0], parts[1]));
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