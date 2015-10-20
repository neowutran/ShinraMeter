using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Tera.Protocol.Game
{
    public class Skill
    {
        public RaceGenderClass RaceGenderClass { get; private set; }
        public int Id { get; private set; }
        public string Name { get; private set; }

        internal Skill(int id, RaceGenderClass raceGenderClass)
        {
            Id = id;
            RaceGenderClass = raceGenderClass;
        }

        internal Skill(int id, RaceGenderClass raceGenderClass, string name)
            : this(id, raceGenderClass)
        {
            Name = name;
        }
    }

    public class SkillDatabase
    {
        private readonly Dictionary<RaceGenderClass, List<Skill>> _data = new Dictionary<RaceGenderClass, List<Skill>>();

        public SkillDatabase(string filename)
        {
            var lines = File.ReadLines(filename);
            var listOfParts = lines.Select(s => s.Split(new[] { ' ' }, 5));
            foreach (var parts in listOfParts)
            {
                var skill = new Skill(int.Parse(parts[0]), new RaceGenderClass(parts[1], parts[2], parts[3]), parts[4]);
                if (!_data.ContainsKey(skill.RaceGenderClass))
                    _data[skill.RaceGenderClass] = new List<Skill>();
                _data[skill.RaceGenderClass].Add(skill);
            }
        }

        public Skill Get(RaceGenderClass raceGenderClass, int skillId)
        {
            var comparer = new ProjectingEqualityComparer<Skill, int>(x => x.Id);
            foreach (var rgc2 in raceGenderClass.Fallbacks())
            {
                if (!_data.ContainsKey(rgc2))
                    continue;

                var searchSkill = new Skill(skillId, raceGenderClass);

                var index = _data[rgc2].BinarySearch(searchSkill, comparer);
                if (index < 0)
                    index = ~index - 1;

                var item = _data[rgc2][index];
                return item;
            }
            return null;
        }

        public string GetName(RaceGenderClass raceGenderClass, int skillId)
        {
            var skill = Get(raceGenderClass, skillId);
            if (skill != null)
                return skill.Name;
            else
                return skillId.ToString();
        }

        private class ProjectingEqualityComparer<T, TKey> : Comparer<T>
        {
            private readonly Comparer<TKey> _keyComparer = Comparer<TKey>.Default;
            private readonly Func<T, TKey> _projection;

            public ProjectingEqualityComparer(Func<T, TKey> projection)
            {
                _projection = projection;
            }

            public override int Compare(T x, T y)
            {
                return _keyComparer.Compare(_projection(x), _projection(y));
            }
        }
    }
}
