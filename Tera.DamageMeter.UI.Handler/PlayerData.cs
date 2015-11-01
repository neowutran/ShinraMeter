using System;

namespace Tera.DamageMeter.UI.Handler
{
    public class PlayerData
    {
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
                return Math.Round(((double)PlayerInfo.Dealt.Damage*100/TotalDamage),1);
            }
        }

        public long TotalDamage { get; set; }

        public PlayerInfo PlayerInfo { get; }
    }
}