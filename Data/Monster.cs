namespace Tera.Data
{
    public class Monster
    {
        public Monster(string id, string name, bool boss)
        {
            Id = id;
            Name = name;
            Boss = boss;
        }

        public string Id { get; private set; }
        public string Name { get; private set; }

        public bool Boss { get; private set; }
    }
}