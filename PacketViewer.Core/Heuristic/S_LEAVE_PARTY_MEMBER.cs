using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Data;
using Tera;
using Tera.Game.Messages;

namespace PacketViewer.Heuristic
{
    class S_LEAVE_PARTY_MEMBER : AbstractPacketHeuristic
    {
        public new void Process(ParsedMessage message) //TODO: find some other check, since it is confused with S_CHANGE_PARTY_MANAGER (not sure)
        {
            base.Process(message);
            if (IsKnown || OpcodeFinder.Instance.IsKnown(message.OpCode))
            {
                if (OpcodeFinder.Instance.GetOpcode(OPCODE) == message.OpCode) { Parse(); }
                return;
            }

            if (message.Payload.Count < 2 + 4 + 4 + 4) return;
            if (!OpcodeFinder.Instance.IsKnown(OpcodeEnum.S_PARTY_MEMBER_LIST)) return;
            if (!OpcodeFinder.Instance.IsKnown(OpcodeEnum.S_CHANGE_PARTY_MANAGER)) return; //avoid parsing that as it has the same structure
            if(!DbUtils.IsPartyFormed()) return;
            var nameOffset = Reader.ReadUInt16();
            if (nameOffset != 14) return;

            var serverId = Reader.ReadUInt32();
            if (BasicTeraData.Instance.Servers.GetServer(serverId) == null) return;

            var playerId = Reader.ReadUInt32();
            var name = "";
            try
            {
                if (Reader.BaseStream.Position != nameOffset - 4) return;
                name = Reader.ReadTeraString();

            }
            catch (Exception e) { return; }
            if (OpcodeFinder.Instance.KnowledgeDatabase.TryGetValue(OpcodeFinder.KnowledgeDatabaseItem.PartyMemberList, out var res))
            {
                var list = (List<PartyMember>)res;
                if (!list.Any(x => x.PlayerId == playerId && x.ServerId == serverId && x.Name == name)) return;
            }
            else return;
            if(OpcodeFinder.Instance.GetMessage(OpcodeFinder.Instance.PacketCount - 1).Direction == MessageDirection.ClientToServer &&
               OpcodeFinder.Instance.GetMessage(OpcodeFinder.Instance.PacketCount - 1).Payload.Count == 8) return;
            OpcodeFinder.Instance.SetOpcode(message.OpCode, OPCODE);
            RemovePartyMember(playerId, serverId, name);

        }

        private void Parse()
        {
            Reader.Skip(2);
            var serverId = Reader.ReadUInt32();
            var playerId = Reader.ReadUInt32();
            var name = Reader.ReadTeraString();
            RemovePartyMember(playerId, serverId, name);

        }
        private void RemovePartyMember(uint playerId, uint serverId, string name)
        {
            if (OpcodeFinder.Instance.KnowledgeDatabase.TryGetValue(OpcodeFinder.KnowledgeDatabaseItem.PartyMemberList, out var res))
            {
                var list = (List<PartyMember>)res;
                if (!list.Any(x => x.PlayerId == playerId && x.ServerId == serverId && x.Name == name)) return;
                var p = list.FirstOrDefault(x => x.PlayerId == playerId && x.ServerId == serverId && x.Name == name);
                list.Remove(p);
                OpcodeFinder.Instance.KnowledgeDatabase[OpcodeFinder.KnowledgeDatabaseItem.PartyMemberList] = list;
            }
        }
    }
}
