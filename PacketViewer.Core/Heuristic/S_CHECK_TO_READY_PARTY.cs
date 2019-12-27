using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Data;
using Tera.Game.Messages;

namespace PacketViewer.Heuristic
{
    class S_CHECK_TO_READY_PARTY : AbstractPacketHeuristic
    {
        private int _elementLength = 2 + 2 + 4 + 4 + 1;
        public new void Process(ParsedMessage message)
        {
            base.Process(message);
            if (IsKnown || OpcodeFinder.Instance.IsKnown(message.OpCode)) return;
            if (!DbUtils.IsPartyFormed()) return;
            if (message.Payload.Count < 2 + 2 + _elementLength * 2) return;
            var count = Reader.ReadUInt16();
            if (count == 0) return;
            if (message.Payload.Count < 2 + 2 + _elementLength * count) return;
            var offset = Reader.ReadUInt16();
            var unk = Reader.ReadUInt32(); //always 0?
            var unk2 = Reader.ReadByte(); //always 0?
            //if (unk != 0) return;
            for (int i = 0; i < count; i++)
            {
                if (offset != Reader.BaseStream.Position + 4) return;
                var currentOffset = Reader.ReadUInt16(); //should match current position
                if (currentOffset != offset) return;
                var nextOffset = Reader.ReadUInt16();
                var serverId = Reader.ReadUInt32();
                if (BasicTeraData.Instance.Servers.GetServer(serverId) == null) return;
                var playerId = Reader.ReadUInt32();
                if (!DbUtils.IsPartyMember(playerId, serverId)) return;
                var status = Reader.ReadByte();
                if (status != 0 && status != 1) return;
                offset = nextOffset;
            }
            OpcodeFinder.Instance.SetOpcode(message.OpCode, OPCODE);
        }
    }
}
