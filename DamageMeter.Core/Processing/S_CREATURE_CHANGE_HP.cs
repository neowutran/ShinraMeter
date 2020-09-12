using Tera.Game.Messages;
using RichPresence = Tera.RichPresence.RichPresence;

namespace DamageMeter.Processing
{
    internal class S_CREATURE_CHANGE_HP
    {
        internal S_CREATURE_CHANGE_HP(SCreatureChangeHp message)
        {
            HudManager.Instance.UpdateBoss(message);
            PacketProcessor.Instance.AbnormalityTracker.Update(message);
            NotifyProcessor.Instance.S_CREATURE_CHANGE_HP(message);
            RichPresence.Instance.HandleBossHp(message);
        }
    }
}
