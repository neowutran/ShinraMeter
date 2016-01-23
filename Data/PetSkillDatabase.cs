using System.Collections.Generic;
using System.IO;
using System.Linq;
using Tera.Game;

namespace Data
{
    // Contains information about skills
    // Currently this is limited to the name of the skill
    public class PetSkillDatabase
    {
        private readonly Dictionary<string, List<UserSkill>> _petSkilldata = new Dictionary<string, List<UserSkill>>();


        public PetSkillDatabase(string folder, string language)
        {
            StreamReader reader;
            try
            {
                reader = new StreamReader(File.OpenRead(folder + "pets-skills-" + language + ".tsv"));
            }
            catch
            {
                return;
            }
            while (!reader.EndOfStream)
            {
                var line = reader.ReadLine();
                if (line == null) continue;
                var values = line.Split('\t');

                var petName = values[0];
                var skillId = int.Parse(values[1]);
                var skillName = values[2];

                var skill = new UserSkill(skillId, PlayerClass.Common, petName, skillName, null);
                if (!_petSkilldata.ContainsKey(petName))
                {
                    _petSkilldata[petName] = new List<UserSkill>();
                }
                _petSkilldata[petName].Add(skill);
            }
        }

        // skillIds are reused across races and class, so we need a RaceGenderClass to disambiguate them
        public string Get(string pet, int skillId)
        {
            if (!_petSkilldata.ContainsKey(pet))
            {
                return "";
            }
            var petSkill = _petSkilldata[pet].FirstOrDefault(skill => skill.Id == skillId);
            return petSkill == null ? "" : petSkill.Hit;
        }
    }
}