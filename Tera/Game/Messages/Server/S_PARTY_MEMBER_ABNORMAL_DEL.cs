namespace Tera.Game.Messages
{
    public class SPartyMemberAbnormalDel : ParsedMessage
    {
        internal SPartyMemberAbnormalDel(TeraMessageReader reader) : base(reader)
        {
            TargetId = reader.ReadEntityId();
            AbnormalityId = reader.ReadInt32();
            
        }


        public int AbnormalityId { get; }

        public EntityId TargetId { get; }
    }
}