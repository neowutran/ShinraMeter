using System;
using System.Collections;

namespace Tera.Game.Messages
{
    public class SAbnormalityRefresh : ParsedMessage
    {
        internal SAbnormalityRefresh(TeraMessageReader reader) : base(reader)
        {
            TargetId = reader.ReadEntityId();
            AbnormalityId = reader.ReadInt32();
            Unknow = reader.ReadInt32();
            Unknow2 = reader.ReadInt32();
            StackCounter = reader.ReadInt32();

//            Console.WriteLine("Target:"+TargetId+";Abnormality:"+AbnormalityId+";Unknow:"+Unknow+";Uknow2:"+Unknow2+";Stack:"+StackCounter);
        }

        private int Unknow { get; set; }

        private int Unknow2 { get; set; }


        private int StackCounter { get; set; }

        private int AbnormalityId { get; set; }

        private EntityId TargetId { get; set; }
    }
}
