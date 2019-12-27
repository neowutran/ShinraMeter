using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Tera.Game.Messages;

namespace PacketViewer.Heuristic
{
    public class C_GET_USER_GUILD_LOGO : AbstractPacketHeuristic
    {
        public new void Process(ParsedMessage message)
        {
            base.Process(message);

            if (IsKnown || OpcodeFinder.Instance.IsKnown(message.OpCode)) { return; }
            if (message.Payload.Count != 8) { return; }

            var playerId = Reader.ReadUInt32();
            var guildId = Reader.ReadUInt32();

            if (!OpcodeFinder.Instance.IsKnown(OpcodeEnum.S_GET_USER_LIST)) { return; }
            if (!OpcodeFinder.Instance.KnowledgeDatabase.TryGetValue(OpcodeFinder.KnowledgeDatabaseItem.Characters, out var currChar))
            {
                throw new Exception("Characters should be available");
            }
            var chList = (Dictionary<uint, Character>)currChar;
            if (!chList.TryGetValue(playerId, out var character)) return;
            if (character.GuildId != guildId) return;

            OpcodeFinder.Instance.SetOpcode(message.OpCode, OPCODE);
        }
    }
}
