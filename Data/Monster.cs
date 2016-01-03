namespace Data
{
    public class Monster
    {
        public Monster(string id, string name, bool boss, string hp)
        {
            Id = id;
            Name = name;
            Boss = boss;
            Hp = hp;
        }

        public string Hp { get; private set; }

        public string Id { get; private set; }
        public string Name { get; private set; }

        public bool Boss { get; private set; }
    }
}