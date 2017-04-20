using Data;

namespace DamageMeter.Processing
{
    internal static class S_USER_STATUS
    {
        internal static void Process(Tera.Game.Messages.SUserStatus message)
        {
            if (BasicTeraData.Instance.WindowData.IdleResetTimeout <= 0) return;
            if (message.User!=NetworkController.Instance.EntityTracker.MeterUser.Id) return;
            if (message.Status != 1) DamageTracker.Instance.LastIdleStartTime = message.Time.Ticks;
        }
    }
}
