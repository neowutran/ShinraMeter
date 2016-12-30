using Data;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using Tera.Game;

namespace DamageMeter.Processing
{
    public static class S_SPAWN_NPC
    {
        public static void Process(Tera.Game.Messages.SpawnNpcServerMessage message)
        {
            NetworkController.Instance.EntityTracker.Update(message);
            DamageTracker.Instance.UpdateEntities(message);

            var npc = NetworkController.Instance.EntityTracker.GetOrNull(message.Id) as NpcEntity;
            if (npc.Info.HuntingZoneId == 950 && npc.Info.TemplateId == 9501)
            {
                var bosses = Database.Database.Instance.AllEntity().Select(x => NetworkController.Instance.EntityTracker.GetOrNull(x)).OfType<NpcEntity>();
                var vergosPhase2Part1 = bosses.First(x => x.Info.HuntingZoneId == 950 && x.Info.TemplateId == 1000);
                var vergosPhase2Part2 = bosses.First(x => x.Info.HuntingZoneId == 950 && x.Info.TemplateId == 2000);
                DataExporter.AutomatedExport(vergosPhase2Part1, NetworkController.Instance.AbnormalityStorage);
                DataExporter.AutomatedExport(vergosPhase2Part2, NetworkController.Instance.AbnormalityStorage);
            }
            if (npc.Info.HuntingZoneId == 950 && npc.Info.TemplateId == 9502)
            {
                var bosses = Database.Database.Instance.AllEntity().Select(x => NetworkController.Instance.EntityTracker.GetOrNull(x)).OfType<NpcEntity>();
                var vergosPhase3 = bosses.First(x => x.Info.HuntingZoneId == 950 && x.Info.TemplateId == 3000);
                DataExporter.AutomatedExport(vergosPhase3, NetworkController.Instance.AbnormalityStorage);
            }
        }
    }
}
