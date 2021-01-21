using Data.Actions;
using Data.Events;
using System.Collections.Generic;

namespace DamageMeter.UI
{
    public class CooldownEventViewModel : BaseEventViewModel
    {
        private int _skillId;
        private bool _resetOnly;

        public int SkillId
        {
            get => _skillId;
            set
            {
                if (_skillId == value) return;
                _skillId = value;
                NotifyPropertyChanged();
            }
        }

        public bool ResetOnly
        {
            get => _resetOnly;
            set
            {
                if (_resetOnly == value) return;
                _resetOnly = value;
                NotifyPropertyChanged();
            }
        }

        public override string Type => "Cooldown event";


        public CooldownEventViewModel(CooldownEvent ev, List<Action> act) : base(ev, act)
        {
        }
    }
}