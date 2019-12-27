using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Tera.Game.Messages;

namespace PacketViewer.Heuristic
{
    class C_REQUEST_CONTRACT : AbstractPacketHeuristic
    {
        public new void Process(ParsedMessage message)
        {
            base.Process(message);
            if (IsKnown || OpcodeFinder.Instance.IsKnown(message.OpCode)) return;
            if (message.Payload.Count < 2+2+2+4+4+4+4+4+1) return;
            var nameOffset = Reader.ReadUInt16();
            var dataOffset = Reader.ReadUInt16();
            var dataCount = Reader.ReadUInt16();
            var type = Reader.ReadUInt32();
            if(type != 4 && type != 35) return; //4 = party invite, 35 = broker nego
            var unk2 = Reader.ReadUInt32();
            var unk3 = Reader.ReadUInt32();
            var unk4 = Reader.ReadUInt32();
            try { var name = Reader.ReadTeraString(); }
            catch (Exception e) { return; }
            if(Reader.BaseStream.Position + dataCount != message.Payload.Count) return;
            OpcodeFinder.Instance.SetOpcode(message.OpCode, OPCODE);
        }
    }
}
