namespace DamageMeter.Skills
{
    public class GroupSkillsStats : SkillsStats
    {
        private readonly Entity _boss;
        private PlayerInfo _playerInfo;


        private SkillsStats _stats;

        public GroupSkillsStats(PlayerInfo playerInfo, Entity boss)
        {
            _boss = boss;
            _playerInfo = playerInfo;
        }

        public new long FirstHit => DamageTracker.Instance.EntitiesFirstHit[_boss];
        public new long LastHit => DamageTracker.Instance.EntitiesLastHit[_boss];

        public new long Interval => LastHit - FirstHit;


        //TODO override ALL skillsStats method
    }
}