using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Tera.Game
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

                var skill = new UserSkill(int.Parse(values[0]), playerClass, values[1]);
                if (!_userSkilldata.ContainsKey(skill.PlayerClass))
                {
                    _userSkilldata[skill.PlayerClass] = new List<UserSkill>();
                }
                _userSkilldata[skill.PlayerClass].Add(skill);
            }
        }

        // skillIds are reused across races and class, so we need a RaceGenderClass to disambiguate them
        public UserSkill Get(UserEntity user, int skillId)
        {
            List<UserSkill> skillsSpecific, skillsCommon;

            _userSkilldata.TryGetValue(user.RaceGenderClass.Class, out skillsSpecific);

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

        public UserSkill GetOrPlaceholder(UserEntity user, int skillId)
        {
            if (user == null)
                throw new ArgumentNullException(nameof(user));

            var existing = Get(user, skillId);
            if (existing != null)
                return existing;

            return new UserSkill(skillId, user.RaceGenderClass.Class, "Unknown " + skillId);
        }

        public string GetName(UserEntity user, int skillId)
        {
            return GetOrPlaceholder(user, skillId).Name;
        }
    }
}