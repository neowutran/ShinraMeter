using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Tera.Game.Messages;

namespace PacketViewer.Heuristic
{
    class C_JOIN_PRIVATE_CHANNEL : AbstractPacketHeuristic
    {
        public static string PrivateChannelName;
        public new void Process(ParsedMessage message)
        {
            base.Process(message);
            if (IsKnown || OpcodeFinder.Instance.IsKnown(message.OpCode)) return;
            if (!OpcodeFinder.Instance.IsKnown(OpcodeEnum.S_LOGIN)) return;
            if (message.Payload.Count < 2 + 2 + 4) return;
            var nameOffset = Reader.ReadUInt16();
            var password = Reader.ReadUInt16();
            if(password.ToString().Length != 4) return;
            if(Reader.BaseStream.Position != nameOffset - 4) return;
            try
            {
                var name = Reader.ReadTeraString(); 
                OpcodeFinder.Instance.SetOpcode(message.OpCode, OPCODE);
                PrivateChannelName = name;
            }
            catch (Exception e) { }
        }
    }
}
