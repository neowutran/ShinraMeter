using System.IO;
using Tera.Game;

namespace Data
{
    public class TeraData
    {
        internal TeraData(string region)
        {
            OpCodeNamer =
                new OpCodeNamer(Path.Combine(BasicTeraData.Instance.ResourceDirectory,
                    $"data/opcodes/opcodes-{region}.txt"));
            var language = GetLanguage(region);

            BasicTeraData.Instance.MonsterDatabase = new NpcDatabase(Path.Combine(BasicTeraData.Instance.ResourceDirectory, "data/"), language);
            BasicTeraData.Instance.PetSkillDatabase = new PetSkillDatabase(Path.Combine(BasicTeraData.Instance.ResourceDirectory, "data/"), language);
            BasicTeraData.Instance.SkillDatabase = new SkillDatabase(Path.Combine(BasicTeraData.Instance.ResourceDirectory, "data/"), language);
            BasicTeraData.Instance.HotDotDatabase = new HotDotDatabase(Path.Combine(BasicTeraData.Instance.ResourceDirectory, "data/"), language);
        }

        public string GetLanguage(string region)
        {
            if (BasicTeraData.Instance.WindowData.Language == "Auto")
            {
                return region == "EU" ? "EU-EN" : region;
            }
            return BasicTeraData.Instance.WindowData.Language;
        }

        public OpCodeNamer OpCodeNamer { get; private set; }
    }
}