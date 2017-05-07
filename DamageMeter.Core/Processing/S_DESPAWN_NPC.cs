using Tera.Game.Messages;

namespace DamageMeter.Processing
{
    internal class S_DESPAWN_NPC
    {
        internal S_DESPAWN_NPC(SDespawnNpc message)
        {
            NetworkController.Instance.AbnormalityTracker.Update(message);
            NotifyProcessor.Instance.DespawnNpc(message);
            DataExporter.AutomatedExport(message, NetworkController.Instance.AbnormalityStorage);
        }
    }
}