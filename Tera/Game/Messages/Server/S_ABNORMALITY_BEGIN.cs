using System;

namespace Tera.Game.Messages
{
    public class SAbnormalityBegin : ParsedMessage
    {
        internal SAbnormalityBegin(TeraMessageReader reader) : base(reader)
        {
            TargetId = reader.ReadEntityId();
            SourceId = reader.ReadEntityId();
            AbnormalityId = reader.ReadInt32();

            Console.WriteLine("target = "+TargetId+";Source:"+SourceId+";Abnormality:"+AbnormalityId);

        }

        private int AbnormalityId { get; set; }
        
           
            
        private EntityId TargetId { get; set; }
        private EntityId SourceId { get; set; }

    }
}
        

