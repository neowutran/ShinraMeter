using System;
using System.IO;
using Tera.Game;

namespace Tera.Data
{
    public class TeraData
    {
        public string Region { get; private set; }
        public OpCodeNamer OpCodeNamer { get; private set; }
        public SkillDatabase SkillDatabase { get; private set; }

        internal TeraData(BasicTeraData basicData, string region)
        {
            Region = region;
            SkillDatabase = new SkillDatabase(Path.Combine(basicData.ResourceDirectory, "user_skills.txt"));
            OpCodeNamer = new OpCodeNamer(Path.Combine(basicData.ResourceDirectory, string.Format("opcodes-{0}.txt", region.ToLowerInvariant())));
        }
    }
}
