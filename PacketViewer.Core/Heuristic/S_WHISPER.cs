using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Tera.Game.Messages;

namespace PacketViewer.Heuristic
{

    public class S_WHISPER : AbstractPacketHeuristic
    {
        public new void Process(ParsedMessage message)
        {
            base.Process(message);

            if (IsKnown || OpcodeFinder.Instance.IsKnown(message.OpCode)) { return; }
            if (message.Payload.Count < 53) { return; }
            var offsetAuthorName = Reader.ReadUInt16();
            var offsetRecipient = Reader.ReadUInt16();
            var offsetMessage = Reader.ReadUInt16();
            var authorId = Reader.ReadInt64();
            var unk1 = Reader.ReadByte(); // Read GPK file if one day this value is required
            var gm = Reader.ReadByte();

            if (gm != 0) // GM nearly never speak, probability of one speaking in general when runnning this tools is near 0
            {
                return;
            }

            var unk2 = Reader.ReadByte(); // Read GPK file if one day this value is required

            if (Reader.BaseStream.Position + 4 != offsetAuthorName)
            {
                return;
            }

            var authorName = "";
            try
            {
                authorName = Reader.ReadTeraString();
            }
            catch
            {
                //Not a string
                return;
            }





            if (Reader.BaseStream.Position + 4 != offsetRecipient)
            {
                return;
            }

            var recipientName = "";
            try
            {
                recipientName = Reader.ReadTeraString();
            }
            catch
            {
                //Not a string
                return;
            }




            if (Reader.BaseStream.Position + 4 != offsetMessage)
            {
                return;
            }

            var messageTxt = "";
            try
            {
                messageTxt = Reader.ReadTeraString();
            }
            catch
            {
                //Not a string
                return;
            }

            if (Reader.BaseStream.Position != Reader.BaseStream.Length) //at this point, we must have reached the end of the stream
            {
                return;
            }
            OpcodeFinder.Instance.SetOpcode(message.OpCode, OPCODE);
            if (C_WHISPER.PossibleOpcode != 0)
            {
                if(messageTxt == C_WHISPER.LastWhisperText && authorName == C_WHISPER.LastWhisperAuthor) OpcodeFinder.Instance.SetOpcode(C_WHISPER.PossibleOpcode, OpcodeEnum.C_WHISPER);
            }
        }

    }
}
