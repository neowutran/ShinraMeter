using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DamageMeter.Skills
{
    public class GroupSkillsStats: SkillsStats
    {

      

        private Entity _boss;
        private PlayerInfo _playerInfo;

        public GroupSkillsStats(PlayerInfo playerInfo, Entity boss)
        {

            _boss = boss;
            _playerInfo = playerInfo;
          
        }

        //TODO override ALL skillsStats method

    }
}
