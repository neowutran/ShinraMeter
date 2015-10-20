using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Tera.Protocol.Game.Messages;

namespace Tera.Protocol.Game
{
    public class Npc : Entity
    {
        public Npc(SpawnNpcServerMessage message)
            :base(message.Id)
        {
        }
    }
}
