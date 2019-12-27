using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Tera.Game.Messages;

namespace PacketViewer.Heuristic
{
    class S_BOSS_GAGE_INFO : AbstractPacketHeuristic
    {
        public new void Process(ParsedMessage message)
        {
            base.Process(message);
            if(IsKnown || OpcodeFinder.Instance.IsKnown(message.OpCode)) return;
            if (message.Payload.Count != 8 + 4 + 4 + 8 + 4 + 4 + 1 + 4 + 4 + 1) return;
            var cid = Reader.ReadUInt64();
            var zoneId = Reader.ReadUInt32();
            var templateId = Reader.ReadUInt32();
            var target = Reader.ReadUInt64();
            var unk1 = Reader.ReadUInt32();
            var hpDiff = Reader.ReadSingle();
            var unk2 = Reader.ReadByte();
            var curHp = Reader.ReadSingle();
            var maxHp = Reader.ReadSingle();
            var unk3 = Reader.ReadByte();

            if (unk3 != 1) return;

            //check that the cid is contained in spawned npcs list
            if(!DbUtils.IsNpcSpawned(cid, zoneId, templateId)) return;
            OpcodeFinder.Instance.SetOpcode(message.OpCode, OPCODE);
        }
    }
}
