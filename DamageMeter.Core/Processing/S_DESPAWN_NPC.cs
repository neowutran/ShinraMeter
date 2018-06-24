using Tera.Game.Messages;
using Tera.RichPresence;

namespace DamageMeter.Processing
{
    internal class S_DESPAWN_NPC
    {
        internal S_DESPAWN_NPC(SDespawnNpc message)
        {
            PacketProcessor.Instance.AbnormalityTracker.Update(message);
            NotifyProcessor.Instance.DespawnNpc(message);
            DataExporter.AutomatedExport(message, PacketProcessor.Instance.AbnormalityStorage);
            RichPresence.Instance.DespawnNpc(message);
        }
    }
}