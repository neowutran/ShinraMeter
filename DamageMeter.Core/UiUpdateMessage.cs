using DamageMeter.Database.Structures;
using Data.Actions.Notify;
using System.Collections.Concurrent;
using System.Collections.Generic;
using DamageMeter.TeraDpsApi;
using Tera.Game;
using Tera.Game.Abnormality;

namespace DamageMeter
{
    public class UiUpdateMessage
    {
        public readonly StatsSummary StatsSummary;
        public readonly Skills Skills;
        public readonly List<NpcEntity> Entities;
        public readonly bool TimedEncounter;
        public readonly AbnormalityStorage Abnormals;
        public readonly ConcurrentDictionary<UploadData, NpcEntity> BossHistory;
        public readonly List<ChatMessage> Chatbox;
        public readonly List<NotifyFlashMessage> Flash;

        public UiUpdateMessage(StatsSummary statsSummary, Skills skills, List<NpcEntity> entities, bool timedEncounter,
            AbnormalityStorage abnormals, ConcurrentDictionary<UploadData, NpcEntity> bossHistory, List<ChatMessage> chatbox,
             List<NotifyFlashMessage> flash)
        {
            StatsSummary = statsSummary;
            Skills = skills;
            Entities = entities;
            TimedEncounter = timedEncounter;
            Abnormals = abnormals;
            BossHistory = bossHistory;
            Chatbox = chatbox;
            Flash = flash;
        }
    }
}
