using Data;
using DiscordRPC;
using Tera.Game.Messages;
using RichPresence = Tera.RichPresence.RichPresence;

namespace DamageMeter.Processing
{
    internal static class S_USER_STATUS
    {
        internal static void Process(SUserStatus message)
        {
            if (message.User != PacketProcessor.Instance.EntityTracker.MeterUser.Id) { return; }

            if (message.Status == 0) RichPresence.Instance.HandleUserIdle();
            
            if (BasicTeraData.Instance.WindowData.IdleResetTimeout <= 0) { return; }
            if (message.Status != 1) { DamageTracker.Instance.LastIdleStartTime = message.Time.Ticks; }
        }
    }
}