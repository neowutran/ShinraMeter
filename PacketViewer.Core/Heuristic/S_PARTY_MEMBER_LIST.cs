using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Data;
using Tera.Game;
using Tera.Game.Messages;

namespace PacketViewer.Heuristic
{
    class S_PARTY_MEMBER_LIST : AbstractPacketHeuristic
    {
        public int LastCount;
        public new void Process(ParsedMessage message)
        {
            base.Process(message);
            if (IsKnown || OpcodeFinder.Instance.IsKnown(message.OpCode))
            {
                if (OpcodeFinder.Instance.GetOpcode(OPCODE) == message.OpCode) { Parse(); }
                return;
            }

            if (message.Payload.Count < 118) return;
            var count = Reader.ReadUInt16();
            var offset = Reader.ReadUInt16();

            Reader.Skip(1 + 1 + 4 + 4 + 2 + 2 + 4 + 4);
            var unk5 = Reader.ReadUInt32();
            if (unk5 != 1) return;
            var unk6 = Reader.ReadUInt32();
            if (unk6 != 1) return;
            var unk7 = Reader.ReadByte();
            if (unk7 != 0) return;
            var unk8 = Reader.ReadUInt32();
            //if (unk8 != 1) return;  //not always 1 (always 257 when logging in while already in party?)
            var unk9 = Reader.ReadByte();
            if (unk9 != 0) return;
            var unk10 = Reader.ReadUInt32();
            if (unk10 != 1) return;
            var unk11 = Reader.ReadByte();
            if (unk11 != 0) return;
            var list = new List<PartyMember>();
            for (int i = 0; i < count; i++)
            {
                Reader.BaseStream.Position = offset - 4;
                Reader.Skip(2);
                offset = Reader.ReadUInt16();
                var nameOffset = Reader.ReadUInt16();
                var serverId = Reader.ReadUInt32();
                if (BasicTeraData.Instance.Servers.GetServer(serverId) == null) return; //assume we're not in IM
                var playerId = Reader.ReadUInt32();
                var level = Reader.ReadUInt32();
                if (level > 65) return;
                var cl = Reader.ReadUInt32();
                if (cl > 12) return;
                Reader.Skip(1);
                var cId = Reader.ReadUInt64();
                Reader.Skip( 4 + 1);
                var laurel = Reader.ReadUInt32();
                if (laurel > 5) return;
                Reader.BaseStream.Position = nameOffset - 4;
                var name = "";
                try { name = Reader.ReadTeraString(); }
                catch { return; }
                var p = new PartyMember(playerId, serverId, name, cId);
                list.Add(p);
            }

            OpcodeFinder.Instance.SetOpcode(message.OpCode, OPCODE);
            UpdatePartyMemberList(list);
            LastCount = count;
        }

        private void Parse()
        {
            var count = Reader.ReadUInt16();
            var offset = Reader.ReadUInt16();

            Reader.Skip(1 + 1 + 4 + 4 + 2 + 2 + 4 + 4 +4+4+1+4+1+4+1);
            var list = new List<PartyMember>();
            for (int i = 0; i < count; i++)
            {
                Reader.BaseStream.Position = offset - 4;
                Reader.Skip(2);
                offset = Reader.ReadUInt16();
                var nameOffset = Reader.ReadUInt16();
                var serverId = Reader.ReadUInt32();
                var playerId = Reader.ReadUInt32();
                var level = Reader.ReadUInt32();
                var cl = Reader.ReadUInt32();
                Reader.Skip(1);
                var cId = Reader.ReadUInt64();
                Reader.Skip(4 + 1);
                var laurel = Reader.ReadUInt32();
                Reader.BaseStream.Position = nameOffset - 4;
                var name = Reader.ReadTeraString(); 
                var p = new PartyMember(playerId, serverId, name, cId);
                list.Add(p);
            }

            UpdatePartyMemberList(list);
            //if(LastCount > count) S_LEAVE_PARTY_MEMBER.Instance.Confirm(); //server doesen't send party list after member leaves, so this doesen't work
            LastCount = count;

        }
        private void UpdatePartyMemberList(List<PartyMember> list)
        {
            OpcodeFinder.Instance.KnowledgeDatabase[OpcodeFinder.KnowledgeDatabaseItem.PartyMemberList] = list;
        }

    }

    public struct PartyMember
    {
        public string Name;
        public uint PlayerId;
        public uint ServerId;
        public ulong Cid;
        public uint MaxHp;
        public uint MaxMp;
        public uint MaxRe;
        public List<uint> Abnormals;
        public PartyMember(uint playerId, uint serverId, string name, ulong cid)
        {
            Name = name;
            PlayerId = playerId;
            ServerId = serverId;
            Cid = cid;
            Abnormals = new List<uint>();
            MaxHp = 0;
            MaxMp = 0;
            MaxRe = 0;
        }
    }
}
