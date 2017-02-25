namespace DamageMeter.Processing
{
    internal class S_SPAWN_USER
    {
        internal S_SPAWN_USER(Tera.Game.Messages.SpawnUserServerMessage message)
        {
            NetworkController.Instance.EntityTracker.Update(message);
            NetworkController.Instance.AbnormalityTracker.Update(message);
            NotifyProcessor.Instance.SpawnUser(message);
        }
    }
}
