using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Tera.Protocol.Game
{
    [Flags]
    public enum SkillResultFlags : int
    {
        Bit0 = 1,
        Heal = 2,
        Bit2 = 4,
        Bit16 = 0x10000,
        Bit18 = 0x40000
    }
}
