using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Tera.Game.Messages;

namespace PacketViewer.Heuristic
{
    class S_REQUEST_CONTRACT : AbstractPacketHeuristic
    {
        public new void Process(ParsedMessage message)
        {
            base.Process(message);
            if (IsKnown || OpcodeFinder.Instance.IsKnown(message.OpCode)) return;
            if (message.Payload.Count < 2+2+2+2+8+8+4+4+4+4+4+4) return;
            if(!OpcodeFinder.Instance.IsKnown(OpcodeEnum.S_LOGIN)) return;
            var senderNameOffset = Reader.ReadUInt16();
            var recipientNameOffset = Reader.ReadUInt16();
            var dataOffset = Reader.ReadUInt16();
            var dataCount = Reader.ReadUInt16();

            var senderId = Reader.ReadUInt64();
            var recipientId = Reader.ReadUInt64();
            var type = Reader.ReadUInt32();
            var id = Reader.ReadUInt32();
            var unk3 = Reader.ReadUInt32();
            var time = Reader.ReadUInt32();
            var senderName = "";
            var recipientName = "";
            try
            {
                Reader.BaseStream.Position = senderNameOffset -4;
                senderName = Reader.ReadTeraString();
                if (senderName.Contains("guildlogo")) return;

            }
            catch { return; }
            try
            {
                Reader.BaseStream.Position = recipientNameOffset -4;
                recipientName = Reader.ReadTeraString();
                if(recipientName.Contains("guildlogo")) return;
            }
            catch { return; }
            var curCharName = ((LoggedCharacter)OpcodeFinder.Instance.KnowledgeDatabase[OpcodeFinder.KnowledgeDatabaseItem.LoggedCharacter]).Name;
            if(senderName != curCharName && recipientName != curCharName) return;
            if(recipientId != DbUtils.GetPlayercId() && senderId != DbUtils.GetPlayercId()) return;
            if(senderName == "" || recipientName == "") return;
            if(Reader.BaseStream.Position + dataCount != message.Payload.Count) return;
            OpcodeFinder.Instance.SetOpcode(message.OpCode, OPCODE);
        }
    }
}
