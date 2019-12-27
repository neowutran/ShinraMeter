using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Tera.Game.Messages;

namespace PacketViewer.Heuristic
{
    class S_USER_PAPERDOLL_INFO : AbstractPacketHeuristic
    {
        public new void Process(ParsedMessage message)
        {
            base.Process(message);
            if (IsKnown || OpcodeFinder.Instance.IsKnown(message.OpCode)) return;
            if(C_REQUEST_USER_PAPERDOLL_INFO.PossibleOpcode == 0) return;

            if (message.Payload.Count < 4000) return;
            Reader.Skip(2 + 2 + 2 + 2 + 2 + 2);
            try
            {
                var nameOffset = Reader.ReadUInt16();
                Reader.BaseStream.Position = nameOffset - 4;
                var name = Reader.ReadTeraString();
                if (name != "" && name.Equals(C_REQUEST_USER_PAPERDOLL_INFO.Name, StringComparison.OrdinalIgnoreCase))
                {
                    OpcodeFinder.Instance.SetOpcode(message.OpCode, OPCODE);
                    C_REQUEST_USER_PAPERDOLL_INFO.Confirm();
                }
            }
            catch (Exception e){return;}

        }
    }
}

