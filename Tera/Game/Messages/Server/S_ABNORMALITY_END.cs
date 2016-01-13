using System;

namespace Tera.Game.Messages
{
    public class SAbnormalityEnd : ParsedMessage
    {
        internal SAbnormalityEnd(TeraMessageReader reader) : base(reader)
        {
            TargetId = reader.ReadEntityId();
            AbnormalityId = reader.ReadInt32();
        }

        private int AbnormalityId { get; set; }
        
        private EntityId TargetId { get; set; }
    }
}
        

