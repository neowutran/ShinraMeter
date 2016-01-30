using System;

namespace DamageMeter.Taken
{
    public class DamageTaken : ICloneable
    {
        public long Damage { get; private set; }

        public int Hits { get; private set; }


        public object Clone()
        {
            var clone = new DamageTaken
            {
                Damage = Damage,
                Hits = Hits
            };
            return clone;
        }

        public void AddDamage(long damage)
        {
            Console.WriteLine("Damage taken");
            Damage += damage;
            Hits++;
        }
    }
}