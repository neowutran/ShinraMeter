using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Tera.Game.Messages;

namespace PacketViewer.Heuristic
{
    class C_ASK_INTERACTIVE : AbstractPacketHeuristic
    {
        public static ushort PossibleOpcode;
        public new void Process(ParsedMessage message)
        {
            return;
            base.Process(message);
            if (IsKnown || OpcodeFinder.Instance.IsKnown(message.OpCode)) return;
            if (message.Payload.Count < 2+4+4+4) return;
            var nameOffset = Reader.ReadUInt16();
            var unk = Reader.ReadUInt32();
            var unk2 = Reader.ReadUInt32();
            if(unk != 1 )return;
            if(unk2 != 26) return;
            try { var name = Reader.ReadTeraString(); }
            catch (Exception e) { return; }
            PossibleOpcode = message.OpCode;
        }

        public static void Confirm()
        {
            OpcodeFinder.Instance.SetOpcode(PossibleOpcode, OpcodeEnum.C_ASK_INTERACTIVE);
            

        }
    }
}
