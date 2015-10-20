using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Tera.Protocol.Game;

namespace Tera.Data
{
    public class TeraData
    {
        private static readonly string Directory = Path.GetDirectoryName(typeof(TeraData).Assembly.Location);
        private readonly Lazy<OpCodeNamer> _opCodeNamer = new Lazy<OpCodeNamer>(() => new OpCodeNamer(Path.Combine(Directory, "opcodefile.txt")));
        private readonly Lazy<SkillDatabase> _skillDatabase = new Lazy<SkillDatabase>(() => new SkillDatabase(Path.Combine(Directory, "user_skills.txt")));
        private readonly Lazy<List<string>> _serverIps = new Lazy<List<string>>(() =>
        {
            return
                File.ReadAllLines(Path.Combine(Directory, "ServerIPs.txt"))
                .Select(s => s.Split(new[] { '#' }, 2).First())
                .Select(s => s.Trim())
                .Where(s => !string.IsNullOrEmpty(s))
                .ToList();

        });

        public OpCodeNamer OpCodeNamer { get { return _opCodeNamer.Value; } }
        public SkillDatabase SkillDatabase { get { return _skillDatabase.Value; } }
        public IEnumerable<string> ServerIps { get { return _serverIps.Value; } }
    }
}
