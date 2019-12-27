using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Tera.Game.Messages;

namespace PacketViewer.Heuristic
{
    class C_SHOW_ITEM_TOOLTIP_EX : AbstractPacketHeuristic
    {
        public new void Process(ParsedMessage message)
        {
            base.Process(message);
            if (IsKnown || OpcodeFinder.Instance.IsKnown(message.OpCode)) return;
            if (message.Payload.Count < 2 + 4 + 8 + 4 + 4 + 4 + 4 + 4 + 4) return;
            var nameOffset = Reader.ReadUInt16();
            var unk1 = Reader.ReadUInt32();
            var uid = Reader.ReadUInt64();
            var unk2 = Reader.ReadUInt32();
            var unk3 = Reader.ReadUInt32();
            var unk4 = Reader.ReadUInt32();
            var unk5 = Reader.ReadUInt32();
            var unk6 = Reader.ReadUInt32();
            if (Reader.BaseStream.Position != nameOffset - 4) return;
            try { var ownerName = Reader.ReadTeraString(); }
            catch (Exception e) { return; }

            OpcodeFinder.Instance.SetOpcode(message.OpCode, OPCODE);
        }
    }
}
