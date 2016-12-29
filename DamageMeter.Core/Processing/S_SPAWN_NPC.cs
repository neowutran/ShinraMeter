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
                var vergosPhase2Part1 = NetworkController.Instance.EntityTracker.FindLastestNpcEntityByAreaAndTemplate(950, 1000);
                var vergosPhase2Part2 = NetworkController.Instance.EntityTracker.FindLastestNpcEntityByAreaAndTemplate(950, 2000);
                DataExporter.Export(vergosPhase2Part1, NetworkController.Instance.AbnormalityStorage, DataExporter.Dest.Excel | DataExporter.Dest.Site);
                DataExporter.Export(vergosPhase2Part2, NetworkController.Instance.AbnormalityStorage, DataExporter.Dest.Excel | DataExporter.Dest.Site);
            }
            if (npc.Info.HuntingZoneId == 950 && npc.Info.TemplateId == 9502)
            {
                var vergosPhase3 = NetworkController.Instance.EntityTracker.FindLastestNpcEntityByAreaAndTemplate(950, 3000);
                DataExporter.Export(vergosPhase3, NetworkController.Instance.AbnormalityStorage, DataExporter.Dest.Excel | DataExporter.Dest.Site);
            }
        }
    }
}
