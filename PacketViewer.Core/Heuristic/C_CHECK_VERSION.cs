using System;
using Tera.Game.Messages;

namespace PacketViewer.Heuristic
{
    public class C_CHECK_VERSION : AbstractPacketHeuristic
    {
        public bool Initialized = false;
        public new void Process(ParsedMessage message)
        {
            base.Process(message);

            var samePacket = IsSamePacket(message.OpCode);
            // We are not supposed to find this packet more than 1 time
            if (Initialized && samePacket)
            {
                throw new Exception("Relogin is not supported yet.");
            }

            Initialized = samePacket;
        }
    }
}
