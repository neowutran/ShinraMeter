using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Tera.Game;
using Tera.Game.Messages;

namespace PacketViewer.Heuristic
{
    class S_USER_BLOCK_LIST : AbstractPacketHeuristic
    {
        public static Dictionary<uint, string> BlockedUsers = new Dictionary<uint, string>();

        public new void Process(ParsedMessage message)
        {
            base.Process(message);
            if (IsKnown || OpcodeFinder.Instance.IsKnown(message.OpCode))
            {
                if(OpcodeFinder.Instance.GetOpcode(OPCODE) == message.OpCode) { Parse();}
                return;
            }
            if (message.Payload.Count < 2+2+4+2+2+4+4+4+4+4) return;
            var count = Reader.ReadUInt16();
            if(count == 0) return;
            var offset = Reader.ReadUInt16();
            var list = new Dictionary<uint, string>();
            for (int i = 0; i < count; i++)
            {
                var curOffset = Reader.ReadUInt16();
                var nextOffset = Reader.ReadUInt16();
                var nameOffset = Reader.ReadUInt16();
                var myNoteOffset = Reader.ReadUInt16();
                var playerId = Reader.ReadUInt32();
                var level = Reader.ReadUInt32();
                if (level > 65) return;
                var race = Reader.ReadUInt32();
                try { var r = (Race) race;}
                catch (Exception e) { return; }
                if (Reader.BaseStream.Position != nameOffset - 4) return;
                string name;
                try { name = Reader.ReadTeraString(); }
                catch (Exception e) { return; }
                if (Reader.BaseStream.Position != myNoteOffset - 4) return;
                try { var myNote = Reader.ReadTeraString(); }
                catch (Exception e) { return; }
                list.Add(playerId, name);
            }
            OpcodeFinder.Instance.SetOpcode(message.OpCode, OPCODE);
            BlockedUsers.Clear();
            
            foreach (var u in list)
            {
                BlockedUsers.Add(u.Key, u.Value);
            }
        }

        private void Parse()
        {
            BlockedUsers.Clear();
            var count = Reader.ReadUInt16();
            var offset = Reader.ReadUInt16();
            for (int i = 0; i < count; i++)
            {
                var curOffset = Reader.ReadUInt16();
                var nextOffset = Reader.ReadUInt16();
                var nameOffset = Reader.ReadUInt16();
                var myNoteOffset = Reader.ReadUInt16();
                var playerId = Reader.ReadUInt32();
                var level = Reader.ReadUInt32();
                var race = Reader.ReadUInt32();
                var name = Reader.ReadTeraString();
                var myNote = Reader.ReadTeraString();
                BlockedUsers.Add(playerId,name);
            }
        }

    }
}
