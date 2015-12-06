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


        public SkillDatabase(string filename)
        {
            foreach (var file in Directory.EnumerateFiles(filename, "*.tsv"))
            {
                var name = Path.GetFileNameWithoutExtension(file);
                var skills = new StreamReader(File.OpenRead(file));
                name = char.ToUpper(name[0]) + name.Substring(1);
                var playerClass = (PlayerClass) Enum.Parse(typeof (PlayerClass), name);
                ParseFile(skills, playerClass);
            }
        }

        private void ParseFile(StreamReader reader, PlayerClass playerClass)
        {
            while (!reader.EndOfStream)
            {
                var line = reader.ReadLine();
                if (line == null) continue;
                var values = line.Split('\t');
                string hitnumber = null;
                bool? isChained = null;
                if (values.Length >= 3 && !string.IsNullOrEmpty(values[2]))
                {
                    hitnumber = values[2];
                }
                if (values.Length >= 4 && !string.IsNullOrEmpty(values[3]))
                {
                    isChained = bool.Parse(values[3]);
                }


                var skill = new UserSkill(int.Parse(values[0]), playerClass, values[1], hitnumber, isChained);
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

            return allSkills.FirstOrDefault(skill => skill.Id == skillId);
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
            return GetOrPlaceholder(user, skillId).Name;
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