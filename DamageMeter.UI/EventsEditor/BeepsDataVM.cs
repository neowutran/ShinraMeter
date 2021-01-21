using Data.Actions.Notify.SoundElements;
using System.Windows.Input;
using Nostrum;

namespace DamageMeter.UI
{
    public class BeepsDataVM : BaseSoundVM
    {
        public SynchronizedObservableCollection<BeepVM> Beeps { get; }

        public ICommand AddBeepCommand { get; }

        public BeepsDataVM(Beeps beeps)
        {
            Beeps = new SynchronizedObservableCollection<BeepVM>();
            BeepVM.DeleteBeepEvent += OnDeleteBeepEvent;

            beeps.BeepList.ForEach(b => Beeps.Add(new BeepVM(b.Frequency, b.Duration)));

            AddBeepCommand = new RelayCommand(_ => Beeps.Add(new BeepVM(200, 500)));
        }

        private void OnDeleteBeepEvent(BeepVM obj)
        {
            Beeps.Remove(obj);
        }
    }
}