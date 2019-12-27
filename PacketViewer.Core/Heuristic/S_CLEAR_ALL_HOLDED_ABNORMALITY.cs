using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Tera.Game.Messages;

namespace PacketViewer.Heuristic
{
    class S_CLEAR_ALL_HOLDED_ABNORMALITY : AbstractPacketHeuristic
    {
        private static bool waiting = false;
        
        public new void Process(ParsedMessage message)
        {
            base.Process(message);
            if (IsKnown || OpcodeFinder.Instance.IsKnown(message.OpCode)) return;
            if (message.Payload.Count != 0) return;
            if(!waiting) return;
            OpcodeFinder.Instance.SetOpcode(message.OpCode, OPCODE);
            S_RETURN_TO_LOBBY.Wait();
            waiting = false;
        }

        public static void Wait()
        {
            waiting = true;
        }
    }
}
