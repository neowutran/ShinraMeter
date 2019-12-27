using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Data;
using Tera.Game.Messages;
using Tera.Game;

namespace PacketViewer.Heuristic
{
    class S_PARTY_MEMBER_INFO : AbstractPacketHeuristic
    {

        private List<uint> PlayersId;

        private bool Parse(ParsedMessage message)
        {
            if (message.Payload.Count < 2 + 2 + 4 + 2 + 4 + 4 + 4 + 4 + 1 + 8 + 4 + 1 + 4 + 4) return false;
            if (!OpcodeFinder.Instance.IsKnown(OpcodeEnum.S_LOGIN)) return false;
            var count = Reader.ReadUInt16();
            if (count > 30 || count < 1) { return false; }
            var offset = Reader.ReadUInt16();
            PlayersId = new List<uint>();
            try
            {
                //var msg = Reader.ReadTeraString();
                var unk5 = Reader.ReadUInt16();

                for (int i = 0; i < count; i++)
                {
                    var pointer = Reader.ReadUInt16();
                    if (i == 0 && pointer != offset) { return false; }
                    var nextOffset = Reader.ReadUInt16();
                    if ((i + 1 == count && nextOffset != 0) || (i + 1 != count && nextOffset == 0)) { return false; }
                    var nameOffset = Reader.ReadUInt16();

                    var playerId = Reader.ReadUInt32();
                    PlayersId.Add(playerId);
                    var clas = Reader.ReadUInt16();
                    if (clas > 12) { return false; }
                    var race = Reader.ReadUInt16();
                    if (race > 5) { return false; }
                    var gender = Reader.ReadUInt16();
                    if (gender > 2) { return false; }
                    var level = Reader.ReadUInt16();
                    if (level > 65) { return false; }
                    var status = Reader.ReadByte();
                    if (status != 1 && status != 0) { return false; }
                    var worldMapGuardId = Reader.ReadUInt32();
                    var areaNameId = Reader.ReadUInt32();
                    var unk = Reader.ReadUInt16();
                    if (unk > 30) { return false; }
                    var unk2 = Reader.ReadByte();
                    var unk3 = Reader.ReadByte();
                    var canInvite = Reader.ReadByte();
                    if (canInvite != 0 && canInvite != 1) { return false; }
                    var unk4 = Reader.ReadUInt16();
                    try { var name = Reader.ReadTeraString(); }
                    catch { return false; }
                    if (nextOffset != 0 && Reader.BaseStream.Position != nextOffset) { return false; }
                }
                if (Reader.BaseStream.Position != Reader.BaseStream.Length) //at this point, we must have reached the end of the stream
                {
                    return false;
                }
            }
            catch { }

            return true;
         }
        
        public new void Process(ParsedMessage message)
        {
            base.Process(message);
            if (IsKnown)
            {
                Parse(message);
                RegisterC_REQUEST_PARTY_INFO();
                return;
            }
            if (OpcodeFinder.Instance.IsKnown(message.OpCode)) return;
            if (!Parse(message)) { return; }
            OpcodeFinder.Instance.SetOpcode(message.OpCode, OPCODE);
            RegisterC_REQUEST_PARTY_INFO();
        }

        private void RegisterC_REQUEST_PARTY_INFO()
        {
            var prevMessage = OpcodeFinder.Instance.GetMessage(OpcodeFinder.Instance.PacketCount - 1);
            if (!OpcodeFinder.Instance.IsKnown(prevMessage.OpCode) && prevMessage.Payload.Count == 4)
            {
                var prevReader = new TeraMessageReader(prevMessage);
                var playerId = prevReader.ReadUInt32();
                if (PlayersId.Contains(playerId))
                {
                    OpcodeFinder.Instance.SetOpcode(prevMessage.OpCode, OpcodeEnum.C_REQUEST_PARTY_INFO);
                }
            }
        }
    }
}
