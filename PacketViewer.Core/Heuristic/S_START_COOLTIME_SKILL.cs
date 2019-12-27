
using Tera.Game.Messages;

namespace PacketViewer.Heuristic
{
    class S_START_COOLTIME_SKILL : AbstractPacketHeuristic
    {
        public new void Process(ParsedMessage message)
        {
            base.Process(message);
            if (IsKnown || OpcodeFinder.Instance.IsKnown(message.OpCode)) return;
            if(message.Payload.Count != 8) return;
            if(!OpcodeFinder.Instance.IsKnown(OpcodeEnum.C_START_SKILL)) return;

            var skill = Reader.ReadUInt32() - 0x04000000;
            var cd = Reader.ReadUInt32();
            if (C_START_SKILL.LatestSkill != skill) return;

            OpcodeFinder.Instance.SetOpcode(message.OpCode, OPCODE);
        }
    }
}
