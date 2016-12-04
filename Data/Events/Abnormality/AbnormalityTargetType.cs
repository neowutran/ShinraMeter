using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Data.Events.Abnormality
{
    public enum AbnormalityTargetType
    {
        Self = 0,
        Party = 1,
        Boss = 2,
        PartySelfExcluded = 3, 
        MyBoss = 4
    }
}
