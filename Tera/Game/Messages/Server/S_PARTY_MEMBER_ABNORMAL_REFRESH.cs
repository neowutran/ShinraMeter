namespace Tera.Game.Messages
{
    public class SPartyMemberAbnormalRefresh:ParsedMessage
    {
        internal SPartyMemberAbnormalRefresh(TeraMessageReader reader) : base(reader)
        {
            PrintRaw();
        }
    }
}
