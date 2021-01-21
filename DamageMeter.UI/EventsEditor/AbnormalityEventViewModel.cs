using Data.Actions;
using Data.Events.Abnormality;
using System.Collections.Generic;

namespace DamageMeter.UI
{
    public class AbnormalityEventViewModel : BaseEventViewModel
    {
        public SynchronizedObservableCollection<AbnormalityVM> Abnormalities { get; }

        private AbnormalityTargetType _target;
        private AbnormalityTriggerType _trigger;
        private int _secondsBeforeTrigger;
        private int _rewarnTimeout;

        public AbnormalityTargetType Target
        {
            get => _target;
            set
            {
                if (_target == value) return;
                _target = value;
                NotifyPropertyChanged();
            }
        }

        public AbnormalityTriggerType Trigger
        {
            get => _trigger;
            set
            {
                if (_trigger == value) return;
                _trigger = value;
                NotifyPropertyChanged();
            }
        }

        public int SecondsBeforeTrigger
        {
            get => _secondsBeforeTrigger;
            set
            {
                if (_secondsBeforeTrigger == value) return;
                _secondsBeforeTrigger = value;
                NotifyPropertyChanged();
            }
        }

        public int RewarnTimeout
        {
            get => _rewarnTimeout;
            set
            {
                if (_rewarnTimeout == value) return;
                _rewarnTimeout = value;
                NotifyPropertyChanged();
            }
        }

        public override string Type => "Abnormality event";

        public AbnormalityEventViewModel(AbnormalityEvent ev, List<Action> act) : base(ev, act)
        {
            Abnormalities = new SynchronizedObservableCollection<AbnormalityVM>();

            _target = ev.Target;
            _trigger = ev.Trigger;

            foreach (var (abId, stacks) in ev.Ids)
            {
                Abnormalities.Add(new AbnormalityVM(abId, stacks));
            }

            foreach (var type in ev.Types)
            {
                Abnormalities.Add(new AbnormalityVM(type));
            }
        }
    }
}