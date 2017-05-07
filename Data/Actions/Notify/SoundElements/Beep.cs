namespace Data.Actions.Notify.SoundElements
{
    public class Beep
    {
        public Beep(int frequency, int duration)
        {
            Frequency = frequency;
            Duration = duration;
        }

        public int Frequency { get; set; }
        public int Duration { get; set; }
    }
}