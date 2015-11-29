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
        public UserSkill(int id, PlayerClass playerClass, string name)
            : base(id, name)
        {
            PlayerClass = playerClass;
        }

        public PlayerClass PlayerClass { get; }

        public override bool Equals(object obj)
        {
            var other = obj as UserSkill;
            if (other == null)
                return false;
            return (Id == other.Id) && (PlayerClass.Equals(other.PlayerClass));
        }

        public override int GetHashCode()
        {
            return Id + PlayerClass.GetHashCode();
        }
    }
}