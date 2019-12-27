using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Tera.Game.Messages;

namespace PacketViewer.Heuristic
{
    class C_INVITE_USER_TO_GUILD : AbstractPacketHeuristic
    {
        public static ushort PossibleOpcode;
        public new void Process(ParsedMessage message)
        {
            base.Process(message);
            if (IsKnown || OpcodeFinder.Instance.IsKnown(message.OpCode)) return;
            if (message.Payload.Count < 2+4+1) return;
            var nameOffset = Reader.ReadUInt16();
            var unk1 = Reader.ReadUInt32();
            var unk2 = Reader.ReadByte();
            if(unk1 != 0) return;
            if(unk2 != 0) return;
            try { var name = Reader.ReadTeraString(); }
            catch (Exception e) { return; }
            PossibleOpcode = message.OpCode;
        }

        public static void Confirm()
        {
            OpcodeFinder.Instance.SetOpcode(PossibleOpcode, OpcodeEnum.C_INVITE_USER_TO_GUILD);
        }
    }
}
