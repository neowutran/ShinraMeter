// Copyright (c) Gothos
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Tera.Game
{
    public class Skill
    {
        public int Id { get; private set; }
        public string Name { get; private set; }

        internal Skill(int id, string name)
        {
            Id = id;
            Name = name;
        }
    }

    public class UserSkill : Skill
    {
        public RaceGenderClass RaceGenderClass { get; private set; }

        public UserSkill(int id, RaceGenderClass raceGenderClass, string name)
            : base(id, name)
        {
            RaceGenderClass = raceGenderClass;
        }

        public override bool Equals(object obj)
        {
            var other = obj as UserSkill;
            if (other == null)
                return false;
            return (Id == other.Id) && (RaceGenderClass.Equals(other.RaceGenderClass));
        }

        public override int GetHashCode()
        {
            return Id + RaceGenderClass.GetHashCode();
        }
    }
}