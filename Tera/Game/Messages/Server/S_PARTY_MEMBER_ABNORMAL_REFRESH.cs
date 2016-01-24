using System;

namespace Tera.Game.Messages
{
    public class SPartyMemberAbnormalRefresh : ParsedMessage
    {
        internal SPartyMemberAbnormalRefresh(TeraMessageReader reader) : base(reader)
        {
         //   PrintRaw();
            TargetId = reader.ReadEntityId();
            AbnormalityId = reader.ReadInt32();
            Duration = reader.ReadInt32();
            Unknow = reader.ReadInt32();
            StackCounter = reader.ReadInt32();

            //Console.WriteLine("Target:"+TargetId+";Abnormality:"+AbnormalityId+";Duration:"+Duration+";Uknow:"+Unknow+";Stack:"+StackCounter);
        }

        public int Duration { get; }

        public int Unknow { get; }


        public int StackCounter { get; }

        public int AbnormalityId { get; }

        public EntityId TargetId { get; }
    }
}