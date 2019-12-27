using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Tera.Game;
using Tera.Game.Messages;

namespace PacketViewer.Heuristic
{
    class S_ADD_BLOCKED_USER : AbstractPacketHeuristic
    {
        public new void Process(ParsedMessage message)
        {
            base.Process(message);
            if (IsKnown || OpcodeFinder.Instance.IsKnown(message.OpCode))
            {
                if (OpcodeFinder.Instance.GetOpcode(OPCODE) == message.OpCode) { Parse(); }
                return;
            }
            if (message.Payload.Count < 2+2+4+4+4+4+4) return;
            if (!OpcodeFinder.Instance.IsKnown(OpcodeEnum.S_LOGIN)) return;
            var nameOffset = Reader.ReadUInt16();
            var myNoteOffset = Reader.ReadUInt16();
            var playerId = Reader.ReadUInt32();
            var level = Reader.ReadUInt32();
            if(level > 65) return;
            var race = Reader.ReadUInt32();
            try { var r = (Race)race; }
            catch{ return; }
            string name;
            if (Reader.BaseStream.Position + 4 != nameOffset)
            {
                return;
            }
            try { name = Reader.ReadTeraString(); }
            catch{ return; }
            if (Reader.BaseStream.Position + 4 != myNoteOffset)
            {
                return;
            }
            try { var myNote = Reader.ReadTeraString(); }
            catch { return; }
            if (Reader.BaseStream.Position != Reader.BaseStream.Length) //at this point, we must have reached the end of the stream
            {
                return;
            }
            if (C_BLOCK_USER.PossibleOpcode == 0) return;
            if (C_BLOCK_USER.LastBlockedUser.Equals(name, StringComparison.OrdinalIgnoreCase))
            {
                if(OpcodeFinder.Instance.GetMessage(OpcodeFinder.Instance.PacketCount - 1).OpCode != C_BLOCK_USER.PossibleOpcode) return;
                OpcodeFinder.Instance.SetOpcode(C_BLOCK_USER.PossibleOpcode, OpcodeEnum.C_BLOCK_USER);
                OpcodeFinder.Instance.SetOpcode(message.OpCode, OPCODE);
                S_USER_BLOCK_LIST.BlockedUsers.Add(playerId, name);
                
            }
        }

        private void Parse()
        {
            var nameOffset = Reader.ReadUInt16();
            var myNoteOffset = Reader.ReadUInt16();
            var playerId = Reader.ReadUInt32();
            Reader.BaseStream.Position = nameOffset - 4;
            var name = Reader.ReadTeraString();
            S_USER_BLOCK_LIST.BlockedUsers.Add(playerId, name);
        }
    }
}
