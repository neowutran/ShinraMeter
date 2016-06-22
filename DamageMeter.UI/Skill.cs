
namespace DamageMeter.UI
{
    internal interface ISkill
    {
        void Update(SkillAggregate skill, Database.Structures.Skills skills, Database.Structures.PlayerDealt playerDealt, Database.Structures.EntityInformation entityInformation, bool timedEncounter);
        string SkillNameIdent();
    }
}