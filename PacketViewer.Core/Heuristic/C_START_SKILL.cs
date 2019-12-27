using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Tera.Game.Messages;

namespace PacketViewer.Heuristic
{
    class C_START_SKILL : AbstractPacketHeuristic
    {
        public static uint LatestSkill;

        public void Parse()
        {
            LatestSkill = Reader.ReadUInt32() - 0x04000000;
        }
        public new void Process(ParsedMessage message)
        {
            base.Process(message);
            if (IsKnown || OpcodeFinder.Instance.IsKnown(message.OpCode))
            {
                if(OpcodeFinder.Instance.GetOpcode(OPCODE) == message.OpCode) { Parse();}
                return;
            }

            if(message.Payload.Count != 4+2+4+4+4+4+4+4+1+1+1+8) return;

            var skill = Reader.ReadUInt32()- 0x04000000; //would need skill database or specific skillId to use
            var w = Reader.ReadUInt16();
            var pos = Reader.ReadVector3f();
            var endPos = Reader.ReadVector3f();
            var unk = Reader.ReadByte(); //always true
            var moving = Reader.ReadByte(); //bool
            var cont = Reader.ReadByte(); //bool
            var target = Reader.ReadUInt64();

            if(unk != 1) return;
            if(moving != 0 && moving != 1) return;
            if(cont != 0 && cont!= 1) return;
            if(!pos.Equals(DbUtils.GetPlayerLocation())) return;
            
            OpcodeFinder.Instance.SetOpcode(message.OpCode, OPCODE);
            LatestSkill = skill;
        }
    }
}
