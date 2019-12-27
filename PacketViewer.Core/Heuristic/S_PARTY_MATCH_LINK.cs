using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Tera.Game.Messages;

namespace PacketViewer.Heuristic
{
    class S_PARTY_MATCH_LINK : AbstractPacketHeuristic
    {
        public new void Process(ParsedMessage message)
        {
            base.Process(message);
            if (IsKnown || OpcodeFinder.Instance.IsKnown(message.OpCode)) return;
            if (message.Payload.Count < 2+2+4+1+1+4+4+4) return;
            var nameOffset = Reader.ReadUInt16();
            var msgOffset = Reader.ReadUInt16();
            var id = Reader.ReadUInt32();
            var unk = Reader.ReadByte();
            var raid = Reader.ReadByte();
            var unk2 = Reader.ReadUInt32();
            if(raid != 0 && raid != 1) return;
            if(unk2 != 65) return;
            try
            {
                if(Reader.BaseStream.Position != nameOffset - 4) return;
                var name = Reader.ReadTeraString();
            }
            catch (Exception e) { return; }
            try
            {
                if (Reader.BaseStream.Position != msgOffset - 4) return;
                var msg = Reader.ReadTeraString();
            }
            catch (Exception e) { return; }
            OpcodeFinder.Instance.SetOpcode(message.OpCode, OPCODE);
        }
    }
}
