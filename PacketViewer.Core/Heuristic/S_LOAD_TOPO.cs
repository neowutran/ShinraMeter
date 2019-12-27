using System.Collections.Generic;
using System.Linq;
using Tera.Game;
using Tera.Game.Messages;

namespace PacketViewer.Heuristic
{
    class S_LOAD_TOPO : AbstractPacketHeuristic
    {
        private static Dictionary<ushort, Vector3f> PossibleMessages = new Dictionary<ushort, Vector3f>();
        //TODO: add parsing and clear spawn lists
        public new void Process(ParsedMessage message)
        {
            base.Process(message);
            if (IsKnown || OpcodeFinder.Instance.IsKnown(message.OpCode)) return;

            if (message.Payload.Count != 4 + 4 + 4 + 4 + 1) return;
            var zone = Reader.ReadUInt32();
            var pos = Reader.ReadVector3f();
            try
            {
                var quick = Reader.ReadBoolean();
            }
            catch { return; }
            if(!PossibleMessages.ContainsKey(message.OpCode)) PossibleMessages.Add(message.OpCode, pos);
        }

        public static void Confirm(Vector3f pos)
        {
            if (PossibleMessages.ContainsValue(pos))
            {
                var opc = PossibleMessages.FirstOrDefault(x => x.Value.Equals(pos)).Key;
                OpcodeFinder.Instance.SetOpcode(opc, OpcodeEnum.S_LOAD_TOPO);
            }
        }
    }
}
