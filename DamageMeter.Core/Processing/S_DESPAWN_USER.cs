namespace DamageMeter.Processing
{
    public class S_DESPAWN_USER
    {
        public S_DESPAWN_USER(Tera.Game.Messages.SDespawnUser message)
        {
            NetworkController.Instance.AbnormalityTracker.Update(message);
            NetworkController.Instance.EntityTracker.Update(message);
        }
    }
}
