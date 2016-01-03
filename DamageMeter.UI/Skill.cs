using DamageMeter.Skills.Skill;

namespace DamageMeter.UI
{
    internal interface ISkill
    {
        void Update(Skill skill, SkillStats stats);
        string SkillNameIdent();
    }
}