using System;

namespace Tera.Game.Messages
{
    public class SPartyMemberChangeHp : ParsedMessage
    {
        internal SPartyMemberChangeHp(TeraMessageReader reader) : base(reader)
        {
            TargetId = reader.ReadEntityId();
            HpRemaining = reader.ReadInt32();
            TotalHp = reader.ReadInt32();
            Unknow3 = reader.ReadInt16();
           // Console.WriteLine("target = " + TargetId + ";Hp left:" + HpRemaining + ";Max HP:" + TotalHp + ";Unknow3:" + Unknow3);
        }

        public int Unknow3 { get; }

        public int HpRemaining { get; }

        public int TotalHp { get; }


        public EntityId TargetId { get; }
    }
}