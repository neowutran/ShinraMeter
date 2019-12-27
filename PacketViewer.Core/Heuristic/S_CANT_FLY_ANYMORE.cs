using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Tera.Game.Messages;

namespace PacketViewer.Heuristic
{
    class S_CANT_FLY_ANYMORE : AbstractPacketHeuristic
    {
        public new void Process(ParsedMessage message)
        {
            base.Process(message);
            if (IsKnown || OpcodeFinder.Instance.IsKnown(message.OpCode)) return;
            if (message.Payload.Count != 0) return;

            if(!OpcodeFinder.Instance.IsKnown(OpcodeEnum.C_PLAYER_FLYING_LOCATION)) return;
            if(!OpcodeFinder.Instance.IsKnown(OpcodeEnum.S_PLAYER_CHANGE_FLIGHT_ENERGY)) return;
            Console.WriteLine(S_PLAYER_CHANGE_FLIGHT_ENERGY.LastEnergy);
            Console.WriteLine(C_PLAYER_FLYING_LOCATION.LastType);
            if(S_PLAYER_CHANGE_FLIGHT_ENERGY.LastEnergy != 0) return; //we should be out of energy if we can't fly anymore
            if(C_PLAYER_FLYING_LOCATION.LastType != 7) return; //we should be in descending if we can't fly anymore
            OpcodeFinder.Instance.SetOpcode(message.OpCode, OPCODE);
        }
    }
}
