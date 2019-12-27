using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Tera.Game.Messages;

namespace PacketViewer.Heuristic
{
    class S_GUILD_TOWER_INFO : AbstractPacketHeuristic
    {
        public new void Process(ParsedMessage message)
        {
            base.Process(message);
            if (IsKnown || OpcodeFinder.Instance.IsKnown(message.OpCode)) return;
            if (message.Payload.Count < 2 + 2 + 8 + 4 + 4) return;
            //check that previous packet is sSpawnNpc with Guild Tower template/zone Ids
            if (OpcodeFinder.Instance.GetMessage(OpcodeFinder.Instance.PacketCount - 1).OpCode != OpcodeFinder.Instance.GetOpcode(OpcodeEnum.S_SPAWN_NPC)) return;

            var unkStringOffset = Reader.ReadUInt16();
            var nameOffset = Reader.ReadUInt16();

            var cId = Reader.ReadUInt64();
            //check that spawnedNpcs list contains this
            if (!DbUtils.IsNpcSpawned(cId)) return;

            var unk = Reader.ReadUInt32(); //should be contained in unkString

            //reader position should be == unkStringOffset here
            if (Reader.BaseStream.Position != unkStringOffset - 4) return;

            try
            {
                var unkString = Reader.ReadTeraString(); //guildlogo_27_20443_70 -- 2nd number seems to match unk
                if (!unkString.StartsWith("guildlogo")) return;
                var s = unkString.Split('_');
                if (Convert.ToInt32(s[2]) != unk) return; //not entirely sure about this
            }
            catch (Exception e) { return; }

            //reader position should be == nameOffset here
            if (Reader.BaseStream.Position != nameOffset - 4) return;

            try
            {
                var guildName = Reader.ReadTeraString();
            }
            catch (Exception e) { return; }
            OpcodeFinder.Instance.SetOpcode(message.OpCode, OPCODE);
        }
    }
}
