using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Tera.Game.Messages;

namespace PacketViewer.Heuristic
{
  
    public class C_WHISPER : AbstractPacketHeuristic
    {
        public static string LastWhisperAuthor;
        public static string LastWhisperText;
        public static ushort PossibleOpcode;
        public new void Process(ParsedMessage message)
        {
            base.Process(message);

            if (IsKnown || OpcodeFinder.Instance.IsKnown(message.OpCode)) { return; }
            if (message.Payload.Count < 10) { return; }
            var offsetTarget = Reader.ReadUInt16();
            var offsetMessage = Reader.ReadUInt16();
          
            if (Reader.BaseStream.Position + 4 != offsetTarget)
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
            //OpcodeFinder.Instance.SetOpcode(message.OpCode, OPCODE); (it gets confused with add friend)
            PossibleOpcode = message.OpCode;
            LastWhisperAuthor = authorName;
            LastWhisperText = messageTxt;
        }

    }
}
