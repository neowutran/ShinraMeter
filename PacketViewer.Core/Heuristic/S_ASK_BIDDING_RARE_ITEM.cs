using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Tera.Game.Messages;

namespace PacketViewer.Heuristic
{
    class S_ASK_BIDDING_RARE_ITEM : AbstractPacketHeuristic
    {
        public new void Process(ParsedMessage message)
        {
            base.Process(message);
            if (IsKnown || OpcodeFinder.Instance.IsKnown(message.OpCode)) return;

            if (message.Payload.Count < 2 + 2 + 4 + 4 + 4 + 4 + 4 + 1 + 4 + 1 + 4) return;
            if(!DbUtils.IsPartyFormed()) return;
            var count = Reader.ReadUInt16();
            var offset = Reader.ReadUInt16();
            var index = Reader.ReadUInt32();
            var unk2 = Reader.ReadUInt32();
            var item = Reader.ReadUInt32();
            var unk3 = Reader.ReadUInt32();
            var unk4 = Reader.ReadUInt32();
            var unk5 = Reader.ReadByte();
            var duration = Reader.ReadUInt32();
            var unk6 = Reader.ReadByte();
            var remaining = Reader.ReadUInt32();
            if(unk5 != 0) return;
            if(unk6 != 0 && unk6 != 1) return;
            if (offset != Reader.BaseStream.Position + 4) return;
            try
            {
                for (int i = 0; i < count; i++)
                {
                    Reader.Skip(8);
                }
            }
            catch (Exception e) { return; }
            if(Reader.BaseStream.Position != message.Payload.Count) return;
            OpcodeFinder.Instance.SetOpcode(message.OpCode, OPCODE);
        }
    }
}
