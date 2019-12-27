using Tera.Game.Messages;

namespace PacketViewer.Heuristic
{
    class S_USER_EFFECT : AbstractPacketHeuristic
    {
        public new void Process(ParsedMessage message)
        {
            base.Process(message);
            if (IsKnown || OpcodeFinder.Instance.IsKnown(message.OpCode)) return;
            if(message.Payload.Count != 8+8+4+4) return;

            var target = Reader.ReadUInt64();
            var source = Reader.ReadUInt64();
            var circle = Reader.ReadUInt32();
            var operation = Reader.ReadUInt32();

            if(circle != 2 && circle != 3) return;
            if(operation != 1 && operation != 2) return;
            if(!DbUtils.IsNpcSpawned(source)) return;
            if(!DbUtils.IsUserSpawned(target) && DbUtils.GetPlayercId() != target) return;

            OpcodeFinder.Instance.SetOpcode(message.OpCode, OPCODE);
        }
    }
}
