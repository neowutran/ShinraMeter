namespace DamageMeter.Processing
{
    public static class Abnormalities
    {

        public static void Update(Tera.Game.Messages.SAbnormalityBegin message)
        {
            NetworkController.Instance.AbnormalityTracker.Update(message);
        }

        public static void Update(Tera.Game.Messages.SAbnormalityEnd message)
        {
            NetworkController.Instance.AbnormalityTracker.Update(message);
        }

        public static void Update(Tera.Game.Messages.SAbnormalityRefresh message)
        {
            NetworkController.Instance.AbnormalityTracker.Update(message);
        }


    }
}
