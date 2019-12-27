using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Tera.Game.Messages;

namespace PacketViewer.Heuristic
{
    class S_SYSTEM_MESSAGE_LOOT_ITEM : AbstractPacketHeuristic
    {
        public new void Process(ParsedMessage message)
        {
            base.Process(message);
            if (IsKnown || OpcodeFinder.Instance.IsKnown(message.OpCode)) return;
            if (message.Payload.Count < 2 + 4 + 4 + 4 + 4 + 4 + 4 + 1 + 1 + 1 + 4) return;
            var offset = Reader.ReadUInt16();
            Reader.Skip(4 + 4 + 4 + 4 + 4 + 4 + 1 + 1 + 1);
            if (Reader.BaseStream.Position + 4 != offset) return;
            try
            {
                var msg = Reader.ReadTeraString();
                if (msg.StartsWith("@"))
                {
                    var i = msg.IndexOf('\v');
                    string smt = "";
                    smt = i != -1 ? msg.Substring(1, i - 1) : msg.Substring(1);
                    if (!ushort.TryParse(smt, out var m)) return;
                }
                else return;
            }
            catch (Exception e) { return; }
            OpcodeFinder.Instance.SetOpcode(message.OpCode, OPCODE);
        }
    }
}
