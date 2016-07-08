using Tera.Game;

namespace DamageMeter.Database.Structures
{
    public class EntityInformation
    {
        public EntityInformation(NpcEntity entity, long totalDamage, long beginTime, long endTime)
        {
            Entity = entity;
            TotalDamage = totalDamage;
            BeginTime = beginTime;
            EndTime = endTime;
        }

        public NpcEntity Entity { get; }
        public long TotalDamage { get; }
        public long BeginTime { get; }
        public long EndTime { get; }

        public long Interval => EndTime - BeginTime;
    }
}