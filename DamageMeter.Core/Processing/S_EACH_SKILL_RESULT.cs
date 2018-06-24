using Data;
using Tera.Game;
using Tera.Game.Messages;
using Tera.RichPresence;

namespace DamageMeter.Processing
{
    internal class S_EACH_SKILL_RESULT
    {
        internal S_EACH_SKILL_RESULT(EachSkillResultServerMessage message)
        {
            PacketProcessor.Instance.EntityTracker.Update(message);
            var skillResult = new SkillResult(message, PacketProcessor.Instance.EntityTracker, PacketProcessor.Instance.PlayerTracker,
                BasicTeraData.Instance.SkillDatabase, BasicTeraData.Instance.PetSkillDatabase, PacketProcessor.Instance.AbnormalityTracker);
            DamageTracker.Instance.Update(skillResult);
            RichPresence.Instance.EachSkillResult(skillResult);
            NotifyProcessor.Instance.UpdateMeterBoss(message);
        }
    }
}