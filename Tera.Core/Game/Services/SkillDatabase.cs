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
            var lines = File.ReadLines(filename);
            var listOfParts = lines.Select(s => s.Split(new[] {' '}, 5));
            foreach (var parts in listOfParts)
            {
                var skill = new UserSkill(int.Parse(parts[0]), new RaceGenderClass(parts[1], parts[2], parts[3]),
                    parts[4]);
                if (!_userSkilldata.ContainsKey(skill.RaceGenderClass))
                    _userSkilldata[skill.RaceGenderClass] = new List<UserSkill>();
                _userSkilldata[skill.RaceGenderClass].Add(skill);
            }
        }

        // skillIds are reused across races and class, so we need a RaceGenderClass to disambiguate them
        public UserSkill Get(UserEntity user, int skillId)
        {
            var raceGenderClass = user.RaceGenderClass;
            var comparer = new Helpers.ProjectingEqualityComparer<Skill, int>(x => x.Id);
            foreach (var rgc2 in raceGenderClass.Fallbacks())
            {
                if (!_userSkilldata.ContainsKey(rgc2))
                    continue;

                var searchSkill = new UserSkill(skillId, raceGenderClass, null);

                var index = _userSkilldata[rgc2].BinarySearch(searchSkill, comparer);
                if (index < 0)
                    index = ~index - 1;
                if (index < 0)
                    continue;

                var item = _userSkilldata[rgc2][index];
                return item;
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