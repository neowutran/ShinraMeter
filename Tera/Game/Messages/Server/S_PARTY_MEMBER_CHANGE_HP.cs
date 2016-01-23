namespace Tera.Game.Messages
{
    public class SPartyMemberChangeHp: ParsedMessage
    {
        internal SPartyMemberChangeHp(TeraMessageReader reader) : base(reader)
        {
            PrintRaw();

        }
    }
}
