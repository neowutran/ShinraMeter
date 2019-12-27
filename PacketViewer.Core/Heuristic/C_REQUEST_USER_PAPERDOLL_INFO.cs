using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Tera.Game.Messages;

namespace PacketViewer.Heuristic
{
    class C_REQUEST_USER_PAPERDOLL_INFO : AbstractPacketHeuristic
    {
        public static ushort PossibleOpcode;
        public static string Name = "";
        public new void Process(ParsedMessage message)
        {
            base.Process(message);
            if (IsKnown || OpcodeFinder.Instance.IsKnown(message.OpCode)) return;

            if (message.Payload.Count < 6) return;
            Reader.Skip(2);
            try
            {
                Name = Reader.ReadTeraString();
            }
            catch { return; }

            PossibleOpcode = message.OpCode;
        }

        public static void Confirm()
        {
            OpcodeFinder.Instance.SetOpcode(PossibleOpcode, OpcodeEnum.C_REQUEST_USER_PAPERDOLL_INFO);
        }
    }
}
