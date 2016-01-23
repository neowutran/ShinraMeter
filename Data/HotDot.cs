namespace Data
{
    public class HotDot
    {
        public enum DotType
        {
            abs = 2, //each tick  HP +=HPChange ; MP += MPChange
            perc = 3 // each tick  HP += MaxHP*HPChange; MP += MaxMP*MPChange
        }

        public HotDot(int id, int effectId, double hp, double mp, DotType method, int time, int tick, string name)
        {
            Id = id;
            EffectId = effectId;
            Hp = hp;
            Mp = mp;
            Method = method;
            Time = time;
            Tick = tick;
            Name = name;
        }

        public int Id { get; }
        public int EffectId { get; }
        public double Hp { get; }
        public double Mp { get; }
        public DotType Method { get; }
        public int Time { get; }
        public int Tick { get; }
        public string Name { get; }
    }
}