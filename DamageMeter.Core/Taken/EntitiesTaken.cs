using System;
using System.Collections.Generic;
using System.Linq;

namespace DamageMeter.Taken
{
    public class EntitiesTaken : ICloneable
    {
        public Dictionary<Entity, DamageTaken> EntitiesStats = new Dictionary<Entity, DamageTaken>();

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

        public object Clone()
        {
            var clone = new EntitiesTaken
            {
                EntitiesStats = EntitiesStats.ToDictionary(i => i.Key, i => (DamageTaken) i.Value.Clone())
            };
            return clone;
        }
    }
}