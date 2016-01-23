namespace Tera.Game.Messages
{
    public class SPartyMemberAbnormalDel:ParsedMessage
    {
        internal SPartyMemberAbnormalDel(TeraMessageReader reader) : base(reader)
        {
            PrintRaw();
        }
    }
}
