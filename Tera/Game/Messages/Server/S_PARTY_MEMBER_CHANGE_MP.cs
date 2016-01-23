namespace Tera.Game.Messages
{
    public class SPartyMemberChangeMp : ParsedMessage
    {
        internal SPartyMemberChangeMp(TeraMessageReader reader) : base(reader)
        {
            PrintRaw();
        }
    }
}
