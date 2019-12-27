using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Tera;
using Tera.Game.Messages;

namespace PacketViewer.Heuristic
{
    class S_AVAILABLE_EVENT_MATCHING_LIST : AbstractPacketHeuristic
    {
        public new void Process(ParsedMessage message)
        {
            base.Process(message);
            if (IsKnown || OpcodeFinder.Instance.IsKnown(message.OpCode)) return;
            if (message.Payload.Count < 2 + 2 + 4 + 4 + 4 + 4 + 4 + 4 + 4 + 1 + 1 + 4 + 4 + 4 + 4 + 4) return;
            if(!OpcodeFinder.Instance.IsKnown(OpcodeEnum.S_LOGIN)) return;
            for (int i = 0; i < 10; i++)
            {
                var msg = OpcodeFinder.Instance.GetMessage(OpcodeFinder.Instance.PacketCount - 1 - i);
                if(msg.Direction == MessageDirection.ServerToClient) continue;
                if (msg.OpCode == OpcodeFinder.Instance.GetOpcode(OpcodeEnum.C_AVAILABLE_EVENT_MATCHING_LIST))
                {
                    OpcodeFinder.Instance.SetOpcode(message.OpCode, OpcodeEnum.S_AVAILABLE_EVENT_MATCHING_LIST);
                }
            }
            //try
            //{
            //    for (int i = 0; i < count; i++)
            //    {
            //        Reader.Skip(2);
            //        var nextEvent = Reader.ReadUInt16();
            //        var arr2count = Reader.ReadUInt16();
            //        var arr2offset = Reader.ReadUInt16();
            //        var rewardscount = Reader.ReadUInt16();
            //        var rewardsoffset = Reader.ReadUInt16();
            //        var arr1count = Reader.ReadUInt16();
            //        var arr1offset = Reader.ReadUInt16();

            //        Reader.Skip(4 + 4 + 4 + 1 + 1 + 1 + 4);

            //        for (int j = 0; j < rewardscount; j++)
            //        {
            //            Reader.Skip(2);
            //            var nextReward = Reader.ReadUInt16();
            //            Reader.Skip(4 + 4);

            //        }
            //        for (int j = 0; j < arr1count; j++)
            //        {
            //            Reader.Skip(2);
            //            var nextArr1Item = Reader.ReadUInt16();
            //            Reader.Skip(4 + 4 + 4);
            //        }

            //        for (int j = 0; j < arr2count; j++)
            //        {
            //            Reader.Skip(2);
            //            var nextArr2Item = Reader.ReadUInt16();
            //            Reader.Skip(4);
            //        }
            //    }
            //}
            //catch (Exception e) { return; }
            //OpcodeFinder.Instance.SetOpcode(message.OpCode, OPCODE);
        }
    }
}
