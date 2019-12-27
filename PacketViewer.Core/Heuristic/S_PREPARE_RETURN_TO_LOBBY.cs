using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Tera;
using Tera.Game.Messages;

namespace PacketViewer.Heuristic
{
    public class S_PREPARE_RETURN_TO_LOBBY : AbstractPacketHeuristic
    {
        public new void Process(ParsedMessage message)
        {
            base.Process(message);

            if (IsKnown || OpcodeFinder.Instance.IsKnown(message.OpCode)) { return; }
            if(!OpcodeFinder.Instance.IsKnown(OpcodeEnum.S_SPAWN_ME)) return;
            if (message.Payload.Count != 4) { return; }
            var time = Reader.ReadInt32();
            if(time == 0) return;
            for (int i = 0; i < 3; i++)
            {
                var p = OpcodeFinder.Instance.GetMessage(OpcodeFinder.Instance.PacketCount - 1 - i);
                if (p.Direction == MessageDirection.ClientToServer && p.Payload.Count == 0)
                {
                    //Return To lobby detection
                    OpcodeFinder.Instance.SetOpcode(p.OpCode, OpcodeEnum.C_RETURN_TO_LOBBY);
                    //S_PREPARE_RETURN_TO_LOBBY
                    OpcodeFinder.Instance.SetOpcode(message.OpCode, OPCODE);
                    S_CLEAR_ALL_HOLDED_ABNORMALITY.Wait();
                    return;
                }
            }
        }
    }
}