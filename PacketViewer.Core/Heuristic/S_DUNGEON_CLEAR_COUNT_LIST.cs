using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Tera.Game.Messages;

namespace PacketViewer.Heuristic
{
    class S_DUNGEON_CLEAR_COUNT_LIST : AbstractPacketHeuristic //  #3
    {
        public static ushort PossibleOpcode;
        public new void Process(ParsedMessage message)
        {
            base.Process(message);
            if (IsKnown || OpcodeFinder.Instance.IsKnown(message.OpCode)) return;
            if (message.Payload.Count < 2 + 2 + 8 + 4 + 4 + 4 + 4 + 4) return;
            var count = Reader.ReadUInt16();
            var offset = Reader.ReadUInt16();
            try
            {
                Reader.BaseStream.Position = offset - 4;
                for (int i = 0; i < count; i++)
                {
                    Reader.Skip(4 + 4 + 4 + 1);
                }
            }
            catch (Exception e) { return; }
            PossibleOpcode = message.OpCode;
            if(OpcodeFinder.Instance.GetMessage(OpcodeFinder.Instance.PacketCount - 1).OpCode != S_NPCGUILD_LIST.PossibleOpcode) return;
            if(OpcodeFinder.Instance.GetMessage(OpcodeFinder.Instance.PacketCount - 2).OpCode != S_DUNGEON_COOL_TIME_LIST.PossibleOpcode) return;
            if(OpcodeFinder.Instance.GetMessage(OpcodeFinder.Instance.PacketCount - 3).OpCode != OpcodeFinder.Instance.GetOpcode(OpcodeEnum.C_REQUEST_USER_ITEMLEVEL_INFO)) return;
            if(OpcodeFinder.Instance.GetMessage(OpcodeFinder.Instance.PacketCount - 4).OpCode != OpcodeFinder.Instance.GetOpcode(OpcodeEnum.C_DUNGEON_CLEAR_COUNT_LIST)) return;
            if(OpcodeFinder.Instance.GetMessage(OpcodeFinder.Instance.PacketCount - 5).OpCode != OpcodeFinder.Instance.GetOpcode(OpcodeEnum.C_NPCGUILD_LIST)) return;
            if(OpcodeFinder.Instance.GetMessage(OpcodeFinder.Instance.PacketCount - 6).OpCode != OpcodeFinder.Instance.GetOpcode(OpcodeEnum.C_DUNGEON_COOL_TIME_LIST)) return;

            S_NPCGUILD_LIST.Confirm();
            S_DUNGEON_COOL_TIME_LIST.Confirm();
            OpcodeFinder.Instance.SetOpcode(PossibleOpcode, OPCODE);

        }
    }
}
