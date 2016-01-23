using System;

namespace Tera.Game.Messages
{
    public class SPartyMemberAbnormalAdd : ParsedMessage
    {
        internal SPartyMemberAbnormalAdd(TeraMessageReader reader) : base(reader)
        {
            TargetId = reader.ReadEntityId();
            AbnormalityId = reader.ReadInt32();
            Duration = reader.ReadInt32();
            Stack = reader.ReadInt32();
            Console.WriteLine("target = " + TargetId + ";Abnormality:" + AbnormalityId + ";Duration:" + Duration +
                              ";Stack:" + Stack);
        }

        public EntityId TargetId { get; }

        public int AbnormalityId { get; }

        public int Duration { get; }
        public int Stack { get; }
    }
}