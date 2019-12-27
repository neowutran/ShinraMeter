using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Tera.Game.Messages;

namespace PacketViewer.Heuristic
{
    class S_JOIN_PRIVATE_CHANNEL : AbstractPacketHeuristic
    {
        public static List<uint> JoinedChannelId = new List<uint>();
        public static bool pending;
        public static ushort PossibleOpcode;
        public new void Process(ParsedMessage message)
        {
            base.Process(message);
            if (IsKnown || OpcodeFinder.Instance.IsKnown(message.OpCode))
            {
                if (OpcodeFinder.Instance.GetOpcode(OPCODE) == message.OpCode || (pending && message.OpCode == PossibleOpcode)) { Parse(); }
                return;
            }

            //if (!OpcodeFinder.Instance.IsKnown(OpcodeEnum.S_LOGIN)) return;
            if (message.Payload.Count < 2 + 2 + 2 + 4 + 4+4) return;
            var count = Reader.ReadUInt16();
            var offset = Reader.ReadUInt16();
            var nameOffset = Reader.ReadUInt16();
            if (nameOffset < 4) return;
            var index = Reader.ReadUInt32();
            var id = Reader.ReadUInt32();
            if (index > 7) return;
            //should check the array here
            string name;
            try
            {
                Reader.BaseStream.Position = nameOffset - 4;
                name = Reader.ReadTeraString();
            }
            catch (Exception e) { return; }
            pending = true;
            PossibleOpcode = message.OpCode;
            JoinedChannelId.Add(id);
            if (C_JOIN_PRIVATE_CHANNEL.PrivateChannelName == null) return;
            if (name != C_JOIN_PRIVATE_CHANNEL.PrivateChannelName) return;
            pending = false;
            Confirm();
        }

        private void Parse()
        {
            Reader.Skip(2+2+2+4);
            var id = Reader.ReadUInt32();
            JoinedChannelId.Add(id);
        }

        public static void Confirm()
        {
            if(OpcodeFinder.Instance.IsKnown(OpcodeEnum.S_JOIN_PRIVATE_CHANNEL)) return;
            pending = false;
            OpcodeFinder.Instance.SetOpcode(PossibleOpcode, OpcodeEnum.S_JOIN_PRIVATE_CHANNEL);
        }
    }
}
