namespace DamageMeter.Taken
{
    public class DamageTaken
    {
        public long Damage { get; private set; }

        public int Hits { get; private set; }

        public void AddDamage(long damage)
        {
            Damage += damage;
            Hits++;
        }
    }
}