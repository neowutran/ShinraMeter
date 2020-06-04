using System.IO;
using Tera.Game;
using Tera.Game.Messages;

namespace Data
{
    public class TeraData
    {
        internal TeraData(string region)
        {
            //OpCodeNamer =
            //    new OpCodeNamer(Path.Combine(BasicTeraData.Instance.ResourceDirectory,
            //        $"data/opcodes/opcodes-{region}.txt"));
            var language = GetLanguage(region);

            BasicTeraData.Instance.MonsterDatabase = new NpcDatabase(Path.Combine(BasicTeraData.Instance.ResourceDirectory, "data/"), language,
                BasicTeraData.Instance.WindowData.DetectBosses);
            BasicTeraData.Instance.PetSkillDatabase = new PetSkillDatabase(Path.Combine(BasicTeraData.Instance.ResourceDirectory, "data/"), language,
                BasicTeraData.Instance.MonsterDatabase);
            BasicTeraData.Instance.SkillDatabase = new SkillDatabase(Path.Combine(BasicTeraData.Instance.ResourceDirectory, "data/"), language);
            BasicTeraData.Instance.HotDotDatabase = new HotDotDatabase(Path.Combine(BasicTeraData.Instance.ResourceDirectory, "data/"), language);
            //BasicTeraData.Instance.QuestInfoDatabase = new QuestInfoDatabase(Path.Combine(BasicTeraData.Instance.ResourceDirectory, "data/"), language);
            BasicTeraData.Instance.MapData = new MapData(Path.Combine(BasicTeraData.Instance.ResourceDirectory, "data/"), language);
        }

        //public OpCodeNamer OpCodeNamer { get; private set; }

        public string GetLanguage(string region)
        {
            if (BasicTeraData.Instance.WindowData.Language == "Auto") return (region.StartsWith("EU") && BasicTeraData.Instance.Servers.Language !=LangEnum.RU ? region+"-":"")+BasicTeraData.Instance.Servers.Language.ToString();
            return BasicTeraData.Instance.WindowData.Language;
        }
    }
}