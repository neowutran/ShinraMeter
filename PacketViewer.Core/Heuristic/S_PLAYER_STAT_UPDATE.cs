using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Tera.Game.Messages;

namespace PacketViewer.Heuristic
{
    class S_PLAYER_STAT_UPDATE : AbstractPacketHeuristic
    {
        public new void Process(ParsedMessage message)
        {
            base.Process(message);
            if (message.Payload.Count != 231) return;
            if (IsKnown || OpcodeFinder.Instance.IsKnown(message.OpCode))
            {
                if (!IsKnown) return;
                Reader.Skip(12);
                var maxHp = Reader.ReadUInt32();
                var maxMp = Reader.ReadUInt32();
                Reader.Skip(4+4+4+4+2+2+2+4+4+4+4+4+4+4+4+4+4+4+4+4+4+4+2+2+2+4+4+4+4+4+4+4+4+4+4+4+2+1+4+4+4+4+4);
                //RE
                var maxRe = Reader.ReadUInt32();
                var bonusRe = Reader.ReadUInt32();

                UpdatePlayerStats(maxHp, maxMp, maxRe+bonusRe);

                return;
            }

            if (!OpcodeFinder.Instance.KnowledgeDatabase.TryGetValue(OpcodeFinder.KnowledgeDatabaseItem.LoggedCharacter, out var result)) return;
            var ch = (LoggedCharacter)result;
            Reader.Skip(12);
            var maxHp2 = Reader.ReadUInt32();
            var maxMp2 = Reader.ReadUInt32();
            Reader.Skip(4 + 4 + 4 + 4 + 2 + 2 + 2 + 4 + 4 + 4 + 4 + 4 + 4 + 4 + 4 + 4 + 4 + 4 + 4 + 4 + 4 + 4 + 2 + 2 + 2 + 4 + 4 + 4 + 4 + 4 + 4 + 4 + 4 + 4 + 4 + 4);
            var lvl1 = Reader.ReadUInt16();
            if (lvl1 != ch.Level) return;
            var unk4 = Reader.ReadByte();
            if (unk4 != 0 && unk4 != 1) return;
            Reader.Skip(4 + 4 + 4 + 4 + 4 +4);
            var maxRe2 = Reader.ReadUInt32();
            var bonusRe2 = Reader.ReadUInt32();

            Reader.Skip(4 + 4 + 4 + 4);
            var unk7 = Reader.ReadUInt16();
            if (unk7 != 0) return;
            Reader.Skip(2);
            //var unk9 = Reader.ReadUInt32(); //not always true apparently
            //if (unk9 != 8000) return;
            Reader.Skip(4);
            var unk10 = Reader.ReadUInt32(); //same for this
            //if (unk10 != 3) return;
            var lvl2 = Reader.ReadUInt16();
            if (lvl2 != ch.Level) return;
            Reader.Skip(4);
            //var unk13 = Reader.ReadUInt32(); //not always true either
            //if (unk13 != 0) return;
            //var unk14 = Reader.ReadSingle(); //same for this
            //if (unk14 != 1) return;
            OpcodeFinder.Instance.SetOpcode(message.OpCode, OPCODE);
            UpdatePlayerStats(maxHp2, maxMp2, maxRe2 + bonusRe2);

        }

        void UpdatePlayerStats(uint maxhp, uint maxmp, uint maxRe)
        {
            var c = ((LoggedCharacter)OpcodeFinder.Instance.KnowledgeDatabase[OpcodeFinder.KnowledgeDatabaseItem.LoggedCharacter]);
            c.MaxHp = maxhp;
            c.MaxMp = maxmp;
            c.MaxSt = maxRe;
            OpcodeFinder.Instance.KnowledgeDatabase.TryRemove(OpcodeFinder.KnowledgeDatabaseItem.LoggedCharacter, out var garbage);
            OpcodeFinder.Instance.KnowledgeDatabase.TryAdd(OpcodeFinder.KnowledgeDatabaseItem.LoggedCharacter, c);
        }
    }
}
