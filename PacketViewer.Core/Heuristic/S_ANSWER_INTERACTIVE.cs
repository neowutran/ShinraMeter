using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Data;
using Tera;
using Tera.Game;
using Tera.Game.Messages;

namespace PacketViewer.Heuristic
{
    class S_ANSWER_INTERACTIVE : AbstractPacketHeuristic
    {
        public new void Process(ParsedMessage message)
        {
            base.Process(message);
            if (IsKnown || OpcodeFinder.Instance.IsKnown(message.OpCode)) return;
            if (message.Payload.Count < 2 + 4 + 4 + 4 + 1 + 1 + 4 + 4) return;
            var nameOffset = Reader.ReadUInt16();
            var unk = Reader.ReadUInt32();
            var model = Reader.ReadUInt32();
            if (model < 10101 || model > 11108) return; //could me made more accurate by checking actual race/gender/class ranges
            var level = Reader.ReadUInt32();
            var hasParty = Reader.ReadByte();
            var hasGuild = Reader.ReadByte();
            var serverId = Reader.ReadUInt32();

            if (level > 65) return;
            if (hasParty != 0 && hasParty != 1) return;
            if (hasGuild != 0 && hasGuild != 1) return;

            //assume it's a player from our same server
            if (BasicTeraData.Instance.Servers.GetServer(serverId) == null) return;
            string name;
            try
            {
                name = Reader.ReadTeraString();
                if (message.Payload.Count != 2 + 4 + 4 + 4 + 1 + 1 + 4 + 2 + 2 * name.Length) return;

            }
            catch (Exception e) { return; }

            OpcodeFinder.Instance.SetOpcode(message.OpCode, OPCODE);
            for (int i = 0; i < 5; i++)
            {
                var msg = OpcodeFinder.Instance.GetMessage(OpcodeFinder.Instance.PacketCount - 1 - i);
                if (msg.Direction == MessageDirection.ClientToServer)
                {
                    var tmr = new TeraMessageReader(msg);
                    try
                    {
                        tmr.Skip(2 + 4 + 4);
                        if (tmr.ReadTeraString() == name)
                        {
                            OpcodeFinder.Instance.SetOpcode(msg.OpCode, OpcodeEnum.C_ASK_INTERACTIVE);
                        }
                    }
                    catch (Exception e) { continue; }
                }
            }
            //if (OpcodeFinder.Instance.GetMessage(OpcodeFinder.Instance.PacketCount - 1).OpCode == C_ASK_INTERACTIVE.PossibleOpcode) { C_ASK_INTERACTIVE.Confirm(); }
        }
    }
}
