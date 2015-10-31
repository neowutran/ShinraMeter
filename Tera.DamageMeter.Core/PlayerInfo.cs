// Copyright (c) Gothos
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.ComponentModel;
using Tera.Game;

namespace Tera.DamageMeter
{
    public class PlayerInfo : INotifyPropertyChanged
    {
        private readonly DamageTracker _tracker;
        public Player Player { get; private set; }

        public string Name { get { return Player.Name; } }
        public PlayerClass Class { get { return Player.Class; } }

        public SkillStats Received { get; private set; }
        public SkillStats Dealt { get; private set; }

        public double DamageFraction { get { return (double)Dealt.Damage / _tracker.TotalDealt.Damage; } }
        public long? Dps { get { return _tracker.Dps(Dealt.Damage); } }

        public PlayerInfo(Player user, DamageTracker tracker)
        {
            _tracker = tracker;
            Player = user;
            Received = new SkillStats();
            Dealt = new SkillStats();
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChangedEventHandler handler = PropertyChanged;
            if (handler != null) handler(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
