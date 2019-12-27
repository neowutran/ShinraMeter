using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Tera.Game.Messages;

namespace PacketViewer.Heuristic
{
    class C_DUNGEON_CLEAR_COUNT_LIST : AbstractPacketHeuristic //   #3
    {
        public static ushort PossibleOpcode;
        public new void Process(ParsedMessage message)
        {
            base.Process(message);
            if (IsKnown || OpcodeFinder.Instance.IsKnown(message.OpCode)) return;
            if (message.Payload.Count < 6) return;
            if (OpcodeFinder.Instance.GetMessage(OpcodeFinder.Instance.PacketCount - 1).Payload.Count != message.Payload.Count) return;
            Reader.Skip(2);
            try
            {
                var name = Reader.ReadTeraString();
                if (OpcodeFinder.Instance.KnowledgeDatabase.ContainsKey(OpcodeFinder.KnowledgeDatabaseItem.LoggedCharacter))
                {
                    if (((LoggedCharacter)OpcodeFinder.Instance.KnowledgeDatabase[OpcodeFinder.KnowledgeDatabaseItem.LoggedCharacter]).Name != name) return;
                }
            }
            catch { return; }

            PossibleOpcode = message.OpCode;

        }
        public static void Confirm()
        {
            OpcodeFinder.Instance.SetOpcode(PossibleOpcode, OpcodeEnum.C_DUNGEON_CLEAR_COUNT_LIST);
        }
    }
}

