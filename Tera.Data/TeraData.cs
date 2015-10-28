using System.IO;
using System.Linq;
using Tera.Game;

namespace Tera.Data
{
    public class TeraData
    {
        internal TeraData(BasicTeraData basicData, string region)
        {
            Region = basicData.Regions.Single(x => x.Key == region);
            SkillDatabase = new SkillDatabase(Path.Combine(basicData.ResourceDirectory, "user_skills.txt"));
            OpCodeNamer =
                new OpCodeNamer(Path.Combine(basicData.ResourceDirectory,
                    string.Format("opcodes-{0}.txt", Region.Version)));
        }

        public Region Region { get; }
        public OpCodeNamer OpCodeNamer { get; private set; }
        public SkillDatabase SkillDatabase { get; private set; }
    }
}