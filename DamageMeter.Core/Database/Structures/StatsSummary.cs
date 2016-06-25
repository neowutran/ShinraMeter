using System.Collections.Generic;

namespace DamageMeter.Database.Structures
{
    public class StatsSummary
    {
        public StatsSummary(List<PlayerDealt> playerDealt, EntityInformation entityInformation)
        {
            PlayerDealt = playerDealt;
            EntityInformation = entityInformation;
        }

        public List<PlayerDealt> PlayerDealt { get; }
        public EntityInformation EntityInformation { get; }
    }
}