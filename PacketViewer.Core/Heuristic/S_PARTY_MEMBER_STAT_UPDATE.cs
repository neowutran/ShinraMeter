using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Tera.Game.Messages;

namespace PacketViewer.Heuristic
{
    class S_PARTY_MEMBER_STAT_UPDATE : AbstractPacketHeuristic
    {
        public new void Process(ParsedMessage message)
        {
            base.Process(message);
            if (IsKnown || OpcodeFinder.Instance.IsKnown(message.OpCode))
            {
                if (OpcodeFinder.Instance.GetOpcode(OPCODE) == message.OpCode) { Parse(); }
                return;
            }
            if (!OpcodeFinder.Instance.IsKnown(OpcodeEnum.S_PARTY_MEMBER_LIST)) return;
            if (message.Payload.Count != 4 + 4 + 4 + 4 + 4 + 4 + 2 + 2 + 2 + 1 + 4 + 4 + 4 + 4) return;
            if (!OpcodeFinder.Instance.KnowledgeDatabase.ContainsKey(OpcodeFinder.KnowledgeDatabaseItem.PartyMemberList)) return;

            var serverId = Reader.ReadUInt32();
            var playerId = Reader.ReadUInt32();
            var curHp = Reader.ReadUInt32();
            var curMp = Reader.ReadUInt32();
            var maxHp = Reader.ReadUInt32();
            var maxMp = Reader.ReadUInt32();
            var level = Reader.ReadUInt16();
            var combat = Reader.ReadUInt16();
            var vitality = Reader.ReadUInt16();
            var alive = Reader.ReadByte();
            var stamina = Reader.ReadUInt32();
            var curRe = Reader.ReadUInt32();
            var maxRe = Reader.ReadUInt32();
            var unk = Reader.ReadUInt32();

            if (curHp > maxHp) return;
            if (curMp > maxMp) return;
            if (curRe > maxRe) return;
            if (combat != 0 && combat != 1) return;
            if (alive != 0 && alive != 1) return;
            if (level > 65) return;

            if (!DbUtils.IsPartyMember(playerId, serverId)) return;
            OpcodeFinder.Instance.SetOpcode(message.OpCode, OPCODE);

            DbUtils.UpdatePartyMemberMaxHp(playerId, serverId, maxHp);
            DbUtils.UpdatePartyMemberMaxMp(playerId, serverId, maxMp);
            DbUtils.UpdatePartyMemberMaxRe(playerId, serverId, maxRe);

            if (OpcodeFinder.Instance.IsKnown(OpcodeEnum.S_LOGOUT_PARTY_MEMBER)) return;

            var msg = OpcodeFinder.Instance.GetMessage(OpcodeFinder.Instance.PacketCount - 1);
            if (msg.OpCode == S_LOGOUT_PARTY_MEMBER.PossibleOpcode)
            {
                if (S_LOGOUT_PARTY_MEMBER.LastServerId == serverId && S_LOGOUT_PARTY_MEMBER.LastPlayerId == playerId)
                {
                    S_LOGOUT_PARTY_MEMBER.Confirm();
                    return;
                }
            }
            //try 2nd last message too
            msg = OpcodeFinder.Instance.GetMessage(OpcodeFinder.Instance.PacketCount - 2);
            if (msg.OpCode == S_LOGOUT_PARTY_MEMBER.PossibleOpcode)
            {
                if (S_LOGOUT_PARTY_MEMBER.LastServerId == serverId && S_LOGOUT_PARTY_MEMBER.LastPlayerId == playerId)
                {
                    S_LOGOUT_PARTY_MEMBER.Confirm();
                    return;
                }
            }
            msg = OpcodeFinder.Instance.GetMessage(OpcodeFinder.Instance.PacketCount - 1);
            if (msg.OpCode == S_PARTY_MEMBER_BUFF_UPDATE.PossibleOpcode)
            {
                if (S_PARTY_MEMBER_BUFF_UPDATE.LastServerId == serverId && S_PARTY_MEMBER_BUFF_UPDATE.LastPlayerId == playerId)
                {
                    S_PARTY_MEMBER_BUFF_UPDATE.Confirm();
                }
            }

        }

        private void Parse()
        {
            var serverId = Reader.ReadUInt32();
            var playerId = Reader.ReadUInt32();
            var curHp = Reader.ReadUInt32();
            var curMp = Reader.ReadUInt32();
            var maxHp = Reader.ReadUInt32();
            var maxMp = Reader.ReadUInt32();
            var level = Reader.ReadUInt16();
            var combat = Reader.ReadUInt16();
            var vitality = Reader.ReadUInt16();
            var alive = Reader.ReadByte();
            var stamina = Reader.ReadUInt32();
            var curRe = Reader.ReadUInt32();
            var maxRe = Reader.ReadUInt32();
            var unk = Reader.ReadUInt32();

            DbUtils.UpdatePartyMemberMaxHp(playerId, serverId, maxHp);
            DbUtils.UpdatePartyMemberMaxMp(playerId, serverId, maxMp);
            DbUtils.UpdatePartyMemberMaxRe(playerId, serverId, maxRe);

            if (!OpcodeFinder.Instance.IsKnown(OpcodeEnum.S_LOGOUT_PARTY_MEMBER))
            {
                var msg = OpcodeFinder.Instance.GetMessage(OpcodeFinder.Instance.PacketCount - 1);
                if (msg.OpCode == S_LOGOUT_PARTY_MEMBER.PossibleOpcode)
                {
                    if (S_LOGOUT_PARTY_MEMBER.LastServerId == serverId && S_LOGOUT_PARTY_MEMBER.LastPlayerId == playerId && alive == 1) S_LOGOUT_PARTY_MEMBER.Confirm();
                }

                //try 2nd last message too
                msg = OpcodeFinder.Instance.GetMessage(OpcodeFinder.Instance.PacketCount - 2);
                if (msg.OpCode == S_LOGOUT_PARTY_MEMBER.PossibleOpcode)
                {
                    if (S_LOGOUT_PARTY_MEMBER.LastServerId == serverId && S_LOGOUT_PARTY_MEMBER.LastPlayerId == playerId && alive == 1) S_LOGOUT_PARTY_MEMBER.Confirm();
                }
            }

            if (!OpcodeFinder.Instance.IsKnown(OpcodeEnum.S_PARTY_MEMBER_ABNORMAL_CLEAR))
            {
                var msg = OpcodeFinder.Instance.GetMessage(OpcodeFinder.Instance.PacketCount - 1);
                if (msg.OpCode == S_PARTY_MEMBER_ABNORMAL_CLEAR.PossibleOpcode)
                {
                    if (S_PARTY_MEMBER_ABNORMAL_CLEAR.LastServerId == serverId && S_PARTY_MEMBER_ABNORMAL_CLEAR.LastPlayerId == playerId && alive == 0) S_PARTY_MEMBER_ABNORMAL_CLEAR.Confirm();
                }

                //try 2nd last message too
                msg = OpcodeFinder.Instance.GetMessage(OpcodeFinder.Instance.PacketCount - 2);
                if (msg.OpCode == S_LOGOUT_PARTY_MEMBER.PossibleOpcode)
                {
                    if (S_PARTY_MEMBER_ABNORMAL_CLEAR.LastServerId == serverId && S_PARTY_MEMBER_ABNORMAL_CLEAR.LastPlayerId == playerId && alive == 0) S_PARTY_MEMBER_ABNORMAL_CLEAR.Confirm();
                }

            }
        }
    }
}
