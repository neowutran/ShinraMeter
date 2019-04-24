﻿using System.Linq;
using Tera.Game;
using Tera.Game.Messages;

namespace DamageMeter.Processing
{
    internal static class S_SPAWN_NPC
    {
        internal static void Process(SpawnNpcServerMessage message)
        {
            PacketProcessor.Instance.EntityTracker.Update(message);
            DamageTracker.Instance.UpdateEntities(message);
            if (message.NpcArea == 950 && message.NpcId == 9501)
            {
                var bosses = Database.Database.Instance.AllEntity().Select(x => PacketProcessor.Instance.EntityTracker.GetOrNull(x)).OfType<NpcEntity>().ToList();
                var vergosPhase2Part1 = bosses.FirstOrDefault(x => x.Info.HuntingZoneId == 950 && x.Info.TemplateId == 1000);
                var vergosPhase2Part2 = bosses.FirstOrDefault(x => x.Info.HuntingZoneId == 950 && x.Info.TemplateId == 2000);
                DataExporter.AutomatedExport(vergosPhase2Part1, PacketProcessor.Instance.AbnormalityStorage);
                DataExporter.AutomatedExport(vergosPhase2Part2, PacketProcessor.Instance.AbnormalityStorage);
            }
            if (message.NpcArea == 950 && message.NpcId == 9502)
            {
                var bosses = Database.Database.Instance.AllEntity().Select(x => PacketProcessor.Instance.EntityTracker.GetOrNull(x)).OfType<NpcEntity>();
                var vergosPhase3 = bosses.FirstOrDefault(x => x.Info.HuntingZoneId == 950 && x.Info.TemplateId == 3000);
                DataExporter.AutomatedExport(vergosPhase3, PacketProcessor.Instance.AbnormalityStorage);
            }

            if ((message.NpcArea == 26 && message.NpcId == 5001) || 
                (message.NpcArea == 39 && message.NpcId == 501) || 
                (message.NpcArea == 51 && message.NpcId == 4001)) { PacketProcessor.Instance.InvokeGeneralDataDisplayChanged(true, message.Id); }
        }
    }
}