using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DamageMeter.Database.Structures
{
    public class StatsSummary
    {

        public List<PlayerDealt> PlayerDealt { get; }
        public EntityInformation EntityInformation { get; }

        public StatsSummary(List<PlayerDealt> playerDealt, EntityInformation entityInformation)
        {
            PlayerDealt = playerDealt;
            EntityInformation = entityInformation;
        }

    
    }
}
