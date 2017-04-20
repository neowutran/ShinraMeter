namespace DamageMeter.Processing
{
    internal static class S_SPAWN_ME
    {
        internal static void Process(Tera.Game.Messages.SpawnMeServerMessage message)
        {
            NetworkController.Instance.AbnormalityTracker.Update(message);
        }
    }
}
