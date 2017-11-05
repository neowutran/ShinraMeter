using Tera.Game.Messages;

namespace DamageMeter.Processing
{
    internal class S_CREATURE_LIFE
    {
        internal S_CREATURE_LIFE(SCreatureLife message)
        {
            PacketProcessor.Instance.EntityTracker.Update(message);
            PacketProcessor.Instance.AbnormalityTracker.Update(message);
        }
    }
}