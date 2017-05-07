using Data.Actions.Notify.SoundElements;

namespace Data.Actions.Notify
{
    public class NotifyAction : Action
    {
        public NotifyAction(SoundInterface sound, Balloon balloon)
        {
            Sound = sound;
            Balloon = balloon;
        }

        public SoundInterface Sound { get; set; }
        public Balloon Balloon { get; set; }
    }
}