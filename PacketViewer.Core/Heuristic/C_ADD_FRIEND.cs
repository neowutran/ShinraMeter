using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Tera.Game.Messages;

namespace PacketViewer.Heuristic
{
    class C_ADD_FRIEND : AbstractPacketHeuristic
    {
        public static ushort PossibleOpcode;
        public static string AddedUserName;
        public new void Process(ParsedMessage message)
        {
            base.Process(message);
            if (IsKnown || OpcodeFinder.Instance.IsKnown(message.OpCode)) return;
            if (message.Payload.Count < 2+2+4+4) return;
            var nameOffset = Reader.ReadUInt16();
            var msgeOffset = Reader.ReadUInt16();

            string name, msg;
            try { name = Reader.ReadTeraString(); }
            catch (Exception e) { return; }
            try { msg = Reader.ReadTeraString(); } //hardcode the default friend message? (region dependent tho)
            catch (Exception e) { return; }
            //if(!OpcodeFinder.Instance.IsKnown(OpcodeEnum.C_WHISPER)) return; //force this packet to be detected only after we have cWhisper?
            PossibleOpcode = message.OpCode;
            AddedUserName = name;
        }

        public static void Confirm()
        {
            OpcodeFinder.Instance.SetOpcode(PossibleOpcode, OpcodeEnum.C_ADD_FRIEND);
        }
    }
}
