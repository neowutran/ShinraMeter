using System.Collections.Concurrent;
using System.Linq;

namespace DamageMeter.Taken
{
    public class EntitiesTaken
    {
        public ConcurrentDictionary<Entity, DamageTaken> EntitiesStats = new ConcurrentDictionary<Entity, DamageTaken>();

        public long Damage
        {
            get
            {
                if (NetworkController.Instance.Encounter == null)
                {
                    return EntitiesStats.Sum(entityStats => entityStats.Value.Damage);
                }
                if (EntitiesStats.ContainsKey(NetworkController.Instance.Encounter))
                {
                    return EntitiesStats[NetworkController.Instance.Encounter].Damage;
                }
                return 0;
            }
        }

        public int Hits
        {
            get
            {
                if (NetworkController.Instance.Encounter == null)
                {
                    return EntitiesStats.Sum(entityStats => entityStats.Value.Hits);
                }
                if (EntitiesStats.ContainsKey(NetworkController.Instance.Encounter))
                {
                    return EntitiesStats[NetworkController.Instance.Encounter].Hits;
                }
                return 0;
            }
        }
    }
}