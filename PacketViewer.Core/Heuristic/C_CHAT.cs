using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Tera.Game.Messages;

namespace PacketViewer.Heuristic
{
    public class C_CHAT : AbstractPacketHeuristic
    {
        public new void Process(ParsedMessage message)
        {
            base.Process(message);

            if (IsKnown || OpcodeFinder.Instance.IsKnown(message.OpCode)) { return; }
            if (message.Payload.Count < 14) { return; }
            var offsetMessage = Reader.ReadUInt16();
            var channel = Reader.ReadUInt32();

            var messageTxt = "";
            try
            {
                messageTxt = Reader.ReadTeraString();
            }
            catch
            {
                //Not a string
                return;
            }
            if (Reader.BaseStream.Position != Reader.BaseStream.Length) //at this point, we must have reached the end of the stream
            {
                return;
            }
            if (!messageTxt.StartsWith("<FONT"))
            {
                return;
            }

            OpcodeFinder.Instance.SetOpcode(message.OpCode, OPCODE);
        }

    }
}
