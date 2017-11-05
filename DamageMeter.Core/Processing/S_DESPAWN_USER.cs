using Tera.Game.Messages;

namespace DamageMeter.Processing
{
    public class S_DESPAWN_USER
    {
        public S_DESPAWN_USER(SDespawnUser message)
        {
            PacketProcessor.Instance.AbnormalityTracker.Update(message);
            PacketProcessor.Instance.EntityTracker.Update(message);
        }
    }
}