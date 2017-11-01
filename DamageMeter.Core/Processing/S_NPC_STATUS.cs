using Data;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Tera.Game;
using Tera.Game.Messages;

namespace DamageMeter.Processing
{
    internal class S_NPC_STATUS
    {
        internal S_NPC_STATUS(SNpcStatus message)
        {
            NetworkController.Instance.AbnormalityTracker.Update(message);


            // Block to add a dummy skill to artificially start the fight. 
            // Added as a work around since ppl start to exploit the systeme get buffed by aggro the boss
            // and then start the fight

            if(NetworkController.Instance.EntityTracker.GetOrNull(message.Target) == null
                || NetworkController.Instance.EntityTracker.GetOrNull(message.Npc) == null) { return; }
            var skillMessage = EachSkillResultServerMessage.DummySkill(message, message.Target, message.Npc);
            var skillResult = new SkillResult(skillMessage, NetworkController.Instance.EntityTracker, NetworkController.Instance.PlayerTracker,
                BasicTeraData.Instance.SkillDatabase, BasicTeraData.Instance.PetSkillDatabase, NetworkController.Instance.AbnormalityTracker);
            DamageTracker.Instance.Update(skillResult);
            NotifyProcessor.Instance.UpdateMeterBoss(message.Npc);
            
        }
    }
}
