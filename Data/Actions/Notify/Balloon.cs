using System.ComponentModel;

namespace Data.Actions.Notify
{
    public class Balloon
    {
        public Balloon(string titleText, string bodyText, int displayTime, EventType evType)
        {
            TitleText = titleText;
            BodyText = bodyText;
            DisplayTime = displayTime;
            EventType = evType;
        }

        public string TitleText { get; set; }
        public string BodyText { get; set; }
        public int DisplayTime { get; set; }
        public EventType EventType { get; set; }
        public string Icon { get; set; }

        public event PropertyChangedEventHandler PropChanged;

        public void Refresh()
        {
            PropChanged?.Invoke(this, new PropertyChangedEventArgs("Refresh"));
        }

        public void UpdateBody(string body)
        {
            BodyText = body;
            PropChanged?.Invoke(this, new PropertyChangedEventArgs("BodyText"));
        }
    }
}