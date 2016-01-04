using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Tera.Game;

namespace Data
{
    // Contains information about skills
    // Currently this is limited to the name of the skill
    public class SkillDatabase
    {
        private readonly Dictionary<PlayerClass, List<UserSkill>> _userSkilldata =
            new Dictionary<PlayerClass, List<UserSkill>>();


        public SkillDatabase(string folder, string language)
        {
           
                var reader = new StreamReader(File.OpenRead(folder+"skills-"+language+".tsv"));
            while (!reader.EndOfStream)
            {
                var line = reader.ReadLine();
                if (line == null) continue;
                var values = line.Split('\t');
                var id = int.Parse(values[0]);
                var race = values[1];
                var gender = values[2];
                PlayerClass playerClass;
                Enum.TryParse(values[3], out playerClass);
                var skillName = values[4];
                var chained = false;
                if (values[5] != "")
                {
                    chained = bool.Parse(values[5]);
                }
                var skillDetail = values[6];
                
                var skill = new UserSkill(id, playerClass, skillName, skillDetail,chained);
                if (!_userSkilldata.ContainsKey(skill.PlayerClass))
                {
                    _userSkilldata[skill.PlayerClass] = new List<UserSkill>();
                }
                _userSkilldata[skill.PlayerClass].Add(skill);
            }
        }

        // skillIds are reused across races and class, so we need a RaceGenderClass to disambiguate them
        public UserSkill Get(PlayerClass user, int skillId)
        {
            List<UserSkill> skillsSpecific, skillsCommon;

            _userSkilldata.TryGetValue(user, out skillsSpecific);

            _userSkilldata.TryGetValue(PlayerClass.Common, out skillsCommon);


            var allSkills = new List<UserSkill>();
            if (skillsCommon != null)
            {
                allSkills = allSkills.Union(skillsCommon).ToList();
            }
            if (skillsSpecific != null)
            {
                allSkills = allSkills.Union(skillsSpecific).ToList();
            }

            var skillResult =  allSkills.FirstOrDefault(skill => skill.Id == skillId);
            return skillResult;
        }

        public UserSkill GetOrPlaceholder(PlayerClass user, int skillId)
        {
            var existing = Get(user, skillId);
            if (existing != null)
                return existing;

            return new UserSkill(skillId, user, "Unknown " + skillId, null, null);
        }

        public string GetName(PlayerClass user, int skillId)
        {
            List<UserSkill> skillsSpecific, skillsCommon;

            _userSkilldata.TryGetValue(user, out skillsSpecific);

            _userSkilldata.TryGetValue(PlayerClass.Common, out skillsCommon);


            var allSkills = new List<UserSkill>();
            if (skillsCommon != null)
            {
                allSkills = allSkills.Union(skillsCommon).ToList();
            }
            if (skillsSpecific != null)
            {
                allSkills = allSkills.Union(skillsSpecific).ToList();
            }

            var researchSkillId = skillId.ToString();
            researchSkillId = researchSkillId.Substring(researchSkillId.Length - 2);
            foreach (var skill in allSkills)
            {
                var skillIdString = skill.Id.ToString();
                var subid = skillIdString.Substring(skillIdString.Length - 2);
                if (researchSkillId == subid)
                {
                    return skill.Name;
                }
            }
            return "Unknow";
        }

        public bool? IsChained(PlayerClass user, int skillId)
        {
            return GetOrPlaceholder(user, skillId).IsChained;
        }

        public string Hit(PlayerClass user, int skillId)
        {
            return GetOrPlaceholder(user, skillId).Hit;
        }
    }
}