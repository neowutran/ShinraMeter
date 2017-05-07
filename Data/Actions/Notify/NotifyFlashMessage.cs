using Data.Actions.Notify.SoundElements;

namespace Data.Actions.Notify
{
    public class NotifyFlashMessage
    {
        public NotifyFlashMessage(SoundInterface sound, Balloon balloon, int priority)
        {
            Sound = sound;
            Balloon = balloon;
            Priority = priority;
        }

        public SoundInterface Sound { get; set; }
        public Balloon Balloon { get; set; }
        public int Priority { get; set; }
    }
}