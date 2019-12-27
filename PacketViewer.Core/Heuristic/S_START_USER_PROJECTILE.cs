using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Tera.Game;
using Tera.Game.Messages;

namespace PacketViewer.Heuristic
{
    class S_START_USER_PROJECTILE : AbstractPacketHeuristic
    {
        public new void Process(ParsedMessage message)
        {
            base.Process(message);
            if (IsKnown || OpcodeFinder.Instance.IsKnown(message.OpCode)) return;
            if(!OpcodeFinder.Instance.IsKnown(OpcodeEnum.S_LOGIN)) return;
            if(!OpcodeFinder.Instance.IsKnown(OpcodeEnum.C_START_SKILL)) return;
            if(!OpcodeFinder.Instance.IsKnown(OpcodeEnum.C_PLAYER_LOCATION)) return;
            if (message.Payload.Count != 8+4+4+8+4+4+4+4+2+4+4+4+2+4+1) return; //last 5B missing in def
            var source = Reader.ReadUInt64();
            var model = Reader.ReadUInt32();
            var unk = Reader.ReadUInt32();
            var id = Reader.ReadUInt64();
            var skill = Reader.ReadUInt32(); //- 0x04000000;
            var startPos = Reader.ReadVector3f();
            var startAngle = Reader.ReadUInt16();
            var endPos = Reader.ReadVector3f();
            var endAngle = Reader.ReadUInt16();
            if(model != DbUtils.GetPlayerModel()) return;
            //TODO: need more checks but atm id = source for player (for gunner at least)
            if(source != DbUtils.GetPlayercId() && id != source) return;
            if(skill/100 != C_START_SKILL.LatestSkill/100) return;
            //if(startPos.DistanceTo((Vector3f)OpcodeFinder.Instance.KnowledgeDatabase[OpcodeFinder.KnowledgeDatabaseItem.PlayerLocation]) > 20) return;
            //TODO: kinda weak like this, should be improved
            OpcodeFinder.Instance.SetOpcode(message.OpCode, OPCODE);
        }
    }
}
