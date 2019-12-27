using System;
using Tera.Game.Messages;

namespace PacketViewer.Heuristic
{
    class S_SYSTEM_MESSAGE : AbstractPacketHeuristic
    {
        public new void Process(ParsedMessage message)
        {
            base.Process(message);
            if (IsKnown || OpcodeFinder.Instance.IsKnown(message.OpCode))
            {
                if (OpcodeFinder.Instance.GetOpcode(OPCODE) == message.OpCode) { Parse(); }
                return;
            }
            if (message.Payload.Count < 2) return;
            var offset = Reader.ReadUInt16();
            try
            {
                var msg = Reader.ReadTeraString();
                if (msg.StartsWith("@"))
                {
                    var i = msg.IndexOf('\v');
                    string smt = "";
                    smt = i != -1 ? msg.Substring(1, i - 1) : msg.Substring(1);
                    if (!ushort.TryParse(smt, out var m)) return;
                }
                else return;
            }
            catch (Exception e) { return; }
            OpcodeFinder.Instance.SetOpcode(message.OpCode, OPCODE);
        }

        private void Parse()
        {
            //we parse sysMsgs to confirm some packets, since they are usually followed by system messages
            Reader.Skip(2);
            var msg = Reader.ReadTeraString();
            if (msg.StartsWith("@970") && msg.Contains("ChannelName")) S_JOIN_PRIVATE_CHANNEL.Confirm();
            if (msg.StartsWith("@3459") && msg.Contains("UserName")) C_ADD_FRIEND.Confirm();
            if (msg.StartsWith("@436") && msg.Contains("UserName")) C_DELETE_FRIEND.Confirm();
            if (msg.StartsWith("@3641") && msg.Contains("userName")) C_INVITE_USER_TO_GUILD.Confirm();

        }
    }
}
