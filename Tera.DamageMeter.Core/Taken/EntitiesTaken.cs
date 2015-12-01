using System.Collections.Concurrent;
using System.Linq;

namespace Tera.DamageMeter.taken
{
    public class EntitiesTaken
    {
        public ConcurrentDictionary<Entity, long> EntitiesStats = new ConcurrentDictionary<Entity, long>();

        public long Damage
        {
            get
            {
              
                    if (NetworkController.Instance.Encounter == null)
                    {
                        return EntitiesStats.Sum(entityStats => entityStats.Value);
                    }
                    if (EntitiesStats.ContainsKey(NetworkController.Instance.Encounter))
                    {
                        return EntitiesStats[NetworkController.Instance.Encounter];
                    }
                    return 0;
                
            }
        }
    }
}