using System.IO;
using System.Linq;
using Tera.Game;

namespace Tera.Data
{
    public class TeraData
    {
        internal TeraData(BasicTeraData basicData, string region)
        {
            SkillDatabase = new SkillDatabase(Path.Combine(BasicTeraData.Instance.ResourceDirectory, "user_skills.txt"));
            Region = basicData.Regions.Single(x => x.Key == region);
            OpCodeNamer =
                new OpCodeNamer(Path.Combine(basicData.ResourceDirectory,
                    string.Format("opcodes-{0}.txt", Region.Version)));
        }

        public Region Region { get; }
        public OpCodeNamer OpCodeNamer { get; private set; }
        public SkillDatabase SkillDatabase { get; private set; }
    }
}