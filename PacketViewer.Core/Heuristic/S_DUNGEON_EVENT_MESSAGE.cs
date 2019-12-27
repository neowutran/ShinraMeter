using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Tera.Game.Messages;

namespace PacketViewer.Heuristic
{
    class S_DUNGEON_EVENT_MESSAGE : AbstractPacketHeuristic
    {

        public new void Process(ParsedMessage message)
        {
            base.Process(message);
            if (IsKnown || OpcodeFinder.Instance.IsKnown(message.OpCode)) return;

            if (message.Payload.Count < 2 + 4 + 1 + 4 + 4) return;

            var offset = Reader.ReadUInt16();
            var unk1 = Reader.ReadUInt32();
            var unk2 = Reader.ReadByte();
            var unk3 = Reader.ReadUInt32();
            if(unk1 != 42) return;
            if(unk2 != 0) return;
            if(unk3 != 27) return;
            if(Reader.BaseStream.Position != offset - 4) return;
            
            try
            {
                var msg = Reader.ReadTeraString();
                if(!msg.Contains("@dungeon")) return;
            }
            catch (Exception e) { return; }

            OpcodeFinder.Instance.SetOpcode(message.OpCode, OPCODE);
        }
    }
}
