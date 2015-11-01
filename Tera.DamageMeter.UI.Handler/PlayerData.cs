namespace Tera.DamageMeter.UI.Handler
{
    public class PlayerData
    {
        public PlayerData(PlayerInfo playerInfo)
        {
            PlayerInfo = playerInfo;
        }

        public double DamageFraction => PlayerInfo.Dealt.Damage/TotalDamage;

        public long TotalDamage { get; set; }

        public PlayerInfo PlayerInfo { get; }
    }
}