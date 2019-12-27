using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Tera.Game.Messages;

namespace PacketViewer.Heuristic
{
    class S_PRIVATE_CHAT : AbstractPacketHeuristic
    {
        public new void Process(ParsedMessage message)
        {
            base.Process(message);
            if (IsKnown || OpcodeFinder.Instance.IsKnown(message.OpCode)) return;
            if (message.Payload.Count < 2+2+4+8+4+4) return;
            if(!OpcodeFinder.Instance.IsKnown(OpcodeEnum.S_JOIN_PRIVATE_CHANNEL)) return;
            var authorOffset = Reader.ReadUInt16();
            var messageOffset = Reader.ReadUInt16();
            var channel = Reader.ReadUInt32();
            if(!S_JOIN_PRIVATE_CHANNEL.JoinedChannelId.Contains(channel)) return;
            var authorId = Reader.ReadUInt64();
            if(Reader.BaseStream.Position != authorOffset - 4) return;
            try { var author = Reader.ReadTeraString(); }
            catch (Exception e) { return; }
            if (Reader.BaseStream.Position != messageOffset - 4) return;
            try
            {
                var msg = Reader.ReadTeraString();
                if(!msg.Contains("<FONT")) return;
            }
            catch (Exception e) { return; }
            OpcodeFinder.Instance.SetOpcode(message.OpCode, OPCODE);
        }
    }
}
