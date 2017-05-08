using System.Collections.Generic;

namespace DamageMeter.Database.Structures
{
    public class StatsSummary
    {
        public StatsSummary(List<PlayerDamageDealt> playerDamageDealt, List<PlayerHealDealt> playerHealDealt, EntityInformation entityInformation)
        {
            PlayerDamageDealt = playerDamageDealt;
            PlayerHealDealt = playerHealDealt;
            EntityInformation = entityInformation;
        }

        public List<PlayerDamageDealt> PlayerDamageDealt { get; }
        public List<PlayerHealDealt> PlayerHealDealt { get; }

        public EntityInformation EntityInformation { get; }
    }
}