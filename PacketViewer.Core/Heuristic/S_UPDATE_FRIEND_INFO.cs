using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Tera.Game;
using Tera.Game.Messages;

namespace PacketViewer.Heuristic
{
    class S_UPDATE_FRIEND_INFO : AbstractPacketHeuristic
    {
        public new void Process(ParsedMessage message)
        {
            base.Process(message);
            if (IsKnown || OpcodeFinder.Instance.IsKnown(message.OpCode)) return;

            if(message.Payload.Count < 2+2+2+4+4+4+4+4+4+4+4+4+1+2+8+4) return; 

            var count = Reader.ReadUInt16();
            if(count != 1) return;
            
            var offset = Reader.ReadUInt16();
            if (offset > 8) return;

            Reader.BaseStream.Position = offset;
            var nameOffset = Reader.ReadUInt16();
            var playerId = Reader.ReadUInt32();
            var level = Reader.ReadUInt32();
            if(level == 0 || level > 65) return;
            var race = Reader.ReadUInt32();
            var clas = Reader.ReadUInt32();
            var gender = Reader.ReadUInt32();
            try
            {
                var rgc = new RaceGenderClass((Race)race, (Gender)gender, (PlayerClass)clas);
            }
            catch (Exception e) { return; }
            var status = Reader.ReadUInt32();
            if(status != 0 && status != 2) return;
            var loc1 = Reader.ReadUInt32();
            var loc2 = Reader.ReadUInt32();
            var loc3 = Reader.ReadUInt32();
            var unk4 = Reader.ReadByte();
            var unk5 = Reader.ReadUInt16();
            var lastOnline = Reader.ReadUInt64();

            if(Reader.BaseStream.Position != nameOffset - 4) return;

            try
            {
                var name = Reader.ReadTeraString();
                
            }
            catch (Exception e) { return; }
            //maybe add checks for friend list
            OpcodeFinder.Instance.SetOpcode(message.OpCode, OPCODE);
        }
    }
}
