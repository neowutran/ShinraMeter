using Tera.Game.Messages;

namespace DamageMeter.Processing
{
    public static class Abnormalities
    {
        public static void Update(SAbnormalityBegin message)
        {
            NetworkController.Instance.AbnormalityTracker.Update(message);
        }

        public static void Update(SAbnormalityEnd message)
        {
            NetworkController.Instance.AbnormalityTracker.Update(message);
        }

        public static void Update(SAbnormalityRefresh message)
        {
            NetworkController.Instance.AbnormalityTracker.Update(message);
        }
    }
}