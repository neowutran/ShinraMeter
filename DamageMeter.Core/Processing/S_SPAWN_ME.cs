using Tera.Game.Messages;

namespace DamageMeter.Processing
{
    internal static class S_SPAWN_ME
    {
        internal static void Process(SpawnMeServerMessage message)
        {
            PacketProcessor.Instance.AbnormalityTracker.Update(message);
        }
    }
}