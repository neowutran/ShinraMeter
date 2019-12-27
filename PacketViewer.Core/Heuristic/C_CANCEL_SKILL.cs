using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Tera.Game.Messages;

namespace PacketViewer.Heuristic
{
    class C_CANCEL_SKILL : AbstractPacketHeuristic
    {      
        public new void Process(ParsedMessage message)
        {
            base.Process(message);
            if (IsKnown || OpcodeFinder.Instance.IsKnown(message.OpCode))
            {
                return;
            }
           
            if(message.Payload.Count != 8) return;

            var skill = Reader.ReadInt32()- 0x04000000; 
            var type = Reader.ReadInt32(); //TODO: add check by type
            if (C_START_SKILL.LatestSkill != skill) return;
            OpcodeFinder.Instance.SetOpcode(message.OpCode, OPCODE);
          
           
           
        }
    }
}
