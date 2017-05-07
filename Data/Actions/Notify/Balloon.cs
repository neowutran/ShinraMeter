namespace Data.Actions.Notify
{
    public class Balloon
    {
        public Balloon(string titleText, string bodyText, int displayTime)
        {
            TitleText = titleText;
            BodyText = bodyText;
            DisplayTime = displayTime;
        }

        public string TitleText { get; set; }
        public string BodyText { get; set; }
        public int DisplayTime { get; set; }
    }
}