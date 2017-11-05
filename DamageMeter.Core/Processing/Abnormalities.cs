using Tera.Game.Messages;

namespace DamageMeter.Processing
{
    public static class Abnormalities
    {
        public static void Update(SAbnormalityBegin message)
        {
            PacketProcessor.Instance.AbnormalityTracker.Update(message);
        }

        public static void Update(SAbnormalityEnd message)
        {
            PacketProcessor.Instance.AbnormalityTracker.Update(message);
        }

        public static void Update(SAbnormalityRefresh message)
        {
            PacketProcessor.Instance.AbnormalityTracker.Update(message);
        }
    }
}