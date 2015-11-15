using System;

namespace Tera.DamageMeter.UI.Handler
{
    public class PlayerData
    {
        public delegate void TotalDamageChangedHandler();

        private long _totalDamage;

        public PlayerData(PlayerInfo playerInfo)
        {
            PlayerInfo = playerInfo;
        }

        public double DamageFraction
        {
            get
            {
                if (TotalDamage == 0)
                {
                    return 0;
                }
                return Math.Round(((double) PlayerInfo.Dealt.Damage*100/TotalDamage), 1);
            }
        }

        public long TotalDamage
        {
            get { return _totalDamage; }
            set
            {
                _totalDamage = value;
                var handler = TotalDamageChanged;
                handler?.Invoke();
            }
        }

        public PlayerInfo PlayerInfo { get; }
        public event TotalDamageChangedHandler TotalDamageChanged;
    }
}