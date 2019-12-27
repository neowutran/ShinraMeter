using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Tera;
using Tera.Game.Messages;

namespace PacketViewer.Heuristic
{
    class S_WORLD_QUEST_VILLAGER_INFO : AbstractPacketHeuristic
    {
        public new void Process(ParsedMessage message)
        {
            base.Process(message);
            if (IsKnown || OpcodeFinder.Instance.IsKnown(message.OpCode)) return;
            if (!OpcodeFinder.Instance.IsKnown(OpcodeEnum.S_PLAYER_STAT_UPDATE)) return;
            //if (message.Payload.Count < 300) return; //should be a big packet, but not sure
            if (OpcodeFinder.Instance.PacketCount > 300) return; //should be received after login 1st time
            //check the last 5 packets for sClearWorldQuestVillagerInfo
            if(message.Payload.Count < 4) { return; }
            var countNpc = Reader.ReadUInt16();
            if(message.Payload.Count != (2 + 2 + countNpc * (2 + 2 + 4 + 4 + 4 + 4 + 4 + 4 + 4))) { return; }
            var offsetNpc = Reader.ReadUInt16();
            if(offsetNpc != Reader.BaseStream.Position + 4) { return; }
            for (var i = 0; i < countNpc; i++)
            {
                var pointer = Reader.ReadUInt16();
                if(i == 0 && pointer != offsetNpc) { return; }
                var nextOffset = Reader.ReadUInt16();
                if(i + 1 == countNpc && nextOffset != 0) { return; }
                if(i +1 != countNpc && nextOffset == 0) { return; }
                Reader.Skip(4 + 4 + 4 + 4 + 4 + 4 + 4);
            }
            if (Reader.BaseStream.Position != Reader.BaseStream.Length) //at this point, we must have reached the end of the stream
            {
                return;
            }
            var prevMessage = OpcodeFinder.Instance.GetMessage(OpcodeFinder.Instance.PacketCount - 1);
            if(prevMessage.Payload.Count != 0) { return; }
            if (OpcodeFinder.Instance.IsKnown(prevMessage.OpCode)) { return; }
            OpcodeFinder.Instance.SetOpcode(prevMessage.OpCode, OpcodeEnum.S_CLEAR_WORLD_QUEST_VILLAGER_INFO);
            OpcodeFinder.Instance.SetOpcode(message.OpCode, OPCODE);


        }
    }
}
