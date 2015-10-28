using System.ComponentModel;
using Tera.DamageMeter.Annotations;

namespace Tera.DamageMeter
{
    public class SkillStats : INotifyPropertyChanged
    {
        private int _crits;
        private long _damage;
        private long _heal;
        private int _hits;

        public long Damage
        {
            get { return _damage; }
            set
            {
                if (value == _damage) return;
                _damage = value;
                OnPropertyChanged("Damage");
            }
        }

        public long Heal
        {
            get { return _heal; }
            set
            {
                if (value == _heal) return;
                _heal = value;
                OnPropertyChanged("Heal");
            }
        }

        public int Hits
        {
            get { return _hits; }
            set
            {
                if (value == _hits) return;
                _hits = value;
                OnPropertyChanged("Hits");
            }
        }

        public int Crits
        {
            get { return _crits; }
            set
            {
                if (value == _crits) return;
                _crits = value;
                OnPropertyChanged("Crits");
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged(string propertyName)
        {
            var handler = PropertyChanged;
            if (handler != null) handler(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}