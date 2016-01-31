using DamageMeter.Skills.Skill;

namespace DamageMeter.UI
{
    internal interface ISkill
    {
        void Update(DamageMeter.Skills.Skill.Skill skill, SkillStats stats);
        string SkillNameIdent();
    }
}