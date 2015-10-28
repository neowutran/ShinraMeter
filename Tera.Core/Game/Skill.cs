namespace Tera.Game
{
    public class Skill
    {
        internal Skill(int id, string name)
        {
            Id = id;
            Name = name;
        }

        public int Id { get; }
        public string Name { get; private set; }
    }

    public class UserSkill : Skill
    {
        public UserSkill(int id, RaceGenderClass raceGenderClass, string name)
            : base(id, name)
        {
            RaceGenderClass = raceGenderClass;
        }

        public RaceGenderClass RaceGenderClass { get; }

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