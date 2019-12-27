using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Tera.Game;
using Tera.Game.Messages;

namespace PacketViewer.Heuristic
{
    class C_PLAYER_FLYING_LOCATION : AbstractPacketHeuristic
    {
        public static uint LastType;
        public new void Process(ParsedMessage message)
        {
            base.Process(message);
            if (IsKnown || OpcodeFinder.Instance.IsKnown(message.OpCode))
            {
                if (OpcodeFinder.Instance.GetOpcode(OPCODE) == message.OpCode) { Parse(); }
                return;
            }
            if (message.Payload.Count != 4+4+4+4+4+4+4+4+4+4+4+4+4+4) return;
            var type = Reader.ReadUInt32();
            if(type != 2 && type != 3 && type != 4 && type != 5 && type != 6 && type != 7 && type != 8) return;
            var pos = Reader.ReadVector3f();
            var dest = Reader.ReadVector3f();
            var time = Reader.ReadUInt32();
            var controls = Reader.ReadVector3f();
            var direction = Reader.ReadVector3f();

            if (controls.X > 1 || controls.Y > 1 || controls.Z > 1) return;
            if (controls.X < -1 || controls.Y < -1 || controls.Z < -1) return;
            if (direction.X > 1 || direction.Y > 1 || direction.Z > 1) return;
            if (direction.X < -1 || direction.Y < -1 || direction.Z < -1) return;

            OpcodeFinder.Instance.SetOpcode(message.OpCode, OPCODE);
            LastType = type;
        }

        private void Parse()
        {
            LastType = Reader.ReadUInt32();
        }
    }
}
