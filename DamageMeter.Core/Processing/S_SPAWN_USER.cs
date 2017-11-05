using Tera.Game.Messages;

namespace DamageMeter.Processing
{
    internal class S_SPAWN_USER
    {
        internal S_SPAWN_USER(SpawnUserServerMessage message)
        {
            PacketProcessor.Instance.EntityTracker.Update(message);
            PacketProcessor.Instance.AbnormalityTracker.Update(message);
            NotifyProcessor.Instance.SpawnUser(message);
        }
    }
}