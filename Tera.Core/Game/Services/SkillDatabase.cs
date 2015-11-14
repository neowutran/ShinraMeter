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
        private readonly Dictionary<RaceGenderClass, List<UserSkill>> _userSkilldata =
            new Dictionary<RaceGenderClass, List<UserSkill>>();


        public SkillDatabase(string filename)
        {
            var reader = new StreamReader(File.OpenRead(filename));
            while (!reader.EndOfStream)
            {
                var line = reader.ReadLine();
                var values = line.Split(';');

                var skill = new UserSkill(int.Parse(values[0]), new RaceGenderClass(values[1], values[2], values[3]),
                    values[4]);
                if (!_userSkilldata.ContainsKey(skill.RaceGenderClass))
                {
                    _userSkilldata[skill.RaceGenderClass] = new List<UserSkill>();
                }
                _userSkilldata[skill.RaceGenderClass].Add(skill);
            }
        }

        // skillIds are reused across races and class, so we need a RaceGenderClass to disambiguate them
        public UserSkill Get(UserEntity user, int skillId)
        {
            List<UserSkill> skillsSpecific, skills1, skills2, skills3, skills4, skills5;

            _userSkilldata.TryGetValue(user.RaceGenderClass, out skillsSpecific);

            _userSkilldata.TryGetValue(
                new RaceGenderClass(Race.Common, user.RaceGenderClass.Gender, user.RaceGenderClass.Class), out skills1);
            _userSkilldata.TryGetValue(new RaceGenderClass(Race.Common, Gender.Common, user.RaceGenderClass.Class),
                out skills2);
            _userSkilldata.TryGetValue(new RaceGenderClass(Race.Common, Gender.Common, PlayerClass.Common), out skills3);
            _userSkilldata.TryGetValue(
                new RaceGenderClass(Race.Common, user.RaceGenderClass.Gender, PlayerClass.Common), out skills4);
            _userSkilldata.TryGetValue(
                new RaceGenderClass(user.RaceGenderClass.Race, Gender.Common, user.RaceGenderClass.Class), out skills5);

            var allSkills = new List<UserSkill>();

            if (skills5 != null)
            {
                allSkills = allSkills.Union(skills5).ToList();
            }
            if (skills4 != null)
            {
                allSkills = allSkills.Union(skills4).ToList();
            }
            if (skills3 != null)
            {
                allSkills = allSkills.Union(skills3).ToList();
            }
            if (skills2 != null)
            {
                allSkills = allSkills.Union(skills2).ToList();
            }
            if (skills1 != null)
            {
                allSkills = allSkills.Union(skills1).ToList();
            }
            if (skillsSpecific != null)
            {
                allSkills = allSkills.Union(skillsSpecific).ToList();
            }

            foreach (var skill in allSkills)
            {
                if (skill.Id == skillId)
                {
                    return skill;
                }
            }

            return null;
        }

        public UserSkill GetOrPlaceholder(UserEntity user, int skillId)
        {
            if (user == null)
                throw new ArgumentNullException("user");

            var existing = Get(user, skillId);
            if (existing != null)
                return existing;

            return new UserSkill(skillId, user.RaceGenderClass, "Unknown " + skillId);
        }

        public string GetName(UserEntity user, int skillId)
        {
            return GetOrPlaceholder(user, skillId).Name;
        }
    }
}