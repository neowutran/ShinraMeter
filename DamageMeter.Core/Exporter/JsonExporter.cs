using System;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using DamageMeter.TeraDpsApi;
using Data;
using Newtonsoft.Json;
using SevenZip;
using Tera.Game;

namespace DamageMeter.Exporter {
    internal class JsonExporter {
        private static readonly BasicTeraData BTD = BasicTeraData.Instance;
        private static readonly object savelock = new object();

        public static void JsonSave(ExtendedStats exdata, string userName = "", bool manual = false) {
            if (!BTD.WindowData.Excel && !manual) { return; }

            var data = exdata.BaseStats;
            var Boss = exdata.Entity.Info;
            /*
            Select save directory
            */
            var fileName = BTD.WindowData.ExcelPathTemplate.Replace("{Area}", Boss.Area.Replace(":", "-")).Replace("{Boss}", Boss.Name.Replace(":", "-"))
                .Replace("{Date}", DateTime.Now.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture))
                .Replace("{Time}", DateTime.Now.ToString("HH-mm-ss", CultureInfo.InvariantCulture))
                .Replace("{User}", string.IsNullOrEmpty(userName) ? "_____" : userName) + ".7z";

            var fname = Path.Combine(BTD.WindowData.ExcelSaveDirectory, fileName);

            /*
            Test if you have access to the user choice directory, if not, switch back to the default save directory
            */
            try { Directory.CreateDirectory(Path.GetDirectoryName(fname)); }
            catch {
                fname = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), fileName);
                Directory.CreateDirectory(Path.GetDirectoryName(fname));
            }

            var file = new FileInfo(fname);
            if (file.Exists) { return; }
            //the only case this can happen is BAM mobtraining, that's not so interesting statistic to deal with more complex file names.

            var jsonData = new JsonData();
            jsonData.areaId = data.areaId;
            jsonData.bossId = data.bossId;
            jsonData.encounterUnixEpoch = data.encounterUnixEpoch.ToString();
            jsonData.fightDuration = data.fightDuration;
            jsonData.partyDps = data.partyDps;
            foreach (var entity in exdata.AllSkills.GetEntities()) {
                var e = entity as NpcEntity;
                if (e != null) {
                    var mob = new JsonMob();
                    mob.entityId = e.Id.Id.ToString();
                    mob.huntingZoneId = e.Info.HuntingZoneId;
                    mob.templateId = e.Info.TemplateId;
                    var abnormals = exdata.Abnormals.Get(e);
                    foreach (var abnormal in abnormals) {
                        foreach (var duration in abnormal.Value.AllDurations()) {
                            mob.abnormals.Add(new JsonAbnormal() {
                                id = abnormal.Key.Id,
                                start = (int) ((duration.Begin - exdata.FirstTick) / TimeSpan.TicksPerMillisecond),
                                end = (int) ((duration.End - exdata.FirstTick) / TimeSpan.TicksPerMillisecond),
                                stack = duration.Stack
                            });
                        }
                    }

                    jsonData.mobs.Add(mob);
                }
            }

            foreach (var member in data.members) {
                var player = PacketProcessor.Instance.PlayerTracker.Get(member.playerServerId, member.playerId);
                var jsonMember = new JsonMember();
                jsonMember.entityId = player.User.Id.Id.ToString();
                jsonMember.templateId = player.RaceGenderClass.Raw;
                jsonMember.playerServerId = member.playerServerId;
                jsonMember.playerId = member.playerId;
                jsonMember.playerServer = member.playerServer;
                jsonMember.playerName = member.playerName;
                jsonMember.playerClass = member.playerClass;
                jsonMember.guild = member.guild;
                jsonMember.aggro = member.aggro;
                jsonMember.healCrit = member.healCrit;
                jsonMember.playerDeaths = member.playerDeaths;
                jsonMember.playerDeathDuration = member.playerDeathDuration;
                jsonMember.playerDps = member.playerDps;
                jsonMember.playerTotalDamage = member.playerTotalDamage;
                jsonMember.playerTotalDamagePercentage = member.playerTotalDamagePercentage;
                jsonMember.playerAverageCritRate = member.playerAverageCritRate;
                var skills = exdata.AllSkills.GetSkillsDealt(player.User, null, true).OrderBy(x => x.Time);
                foreach (var skill in skills) {
                    var jsonSkill = new JsonSkill();
                    jsonSkill.time = (int) ((skill.Time - exdata.FirstTick) / TimeSpan.TicksPerMillisecond);
                    jsonSkill.type = (int) skill.Type;
                    jsonSkill.crit = skill.Critic;
                    jsonSkill.dot = skill.HotDot;
                    jsonSkill.skillId = BTD.SkillDatabase.GetSkillByPetName(skill.Pet?.Name, player.RaceGenderClass)?.Id ?? skill.SkillId;
                    jsonSkill.amount = skill.Amount.ToString();
                    jsonSkill.target = skill.Target.Id.Id== ulong.MaxValue ? null : skill.Target.Id.Id.ToString();
                    jsonMember.dealtSkillLog.Add(jsonSkill);
                }

                skills = exdata.AllSkills.GetSkillsReceived(player.User, true).OrderBy(x => x.Time);
                foreach (var skill in skills) {
                    var jsonSkill = new JsonSkill();
                    jsonSkill.time = (int) ((skill.Time - exdata.FirstTick) / TimeSpan.TicksPerMillisecond);
                    jsonSkill.type = (int) skill.Type;
                    jsonSkill.crit = skill.Critic;
                    jsonSkill.dot = skill.HotDot;
                    jsonSkill.skillId = BTD.SkillDatabase.GetSkillByPetName(skill.Pet?.Name, player.RaceGenderClass)?.Id ?? skill.SkillId;
                    jsonSkill.amount = skill.Amount.ToString();
                    jsonSkill.source = skill.Source.Id.Id == ulong.MaxValue ? null : skill.Source.Id.Id.ToString();
                    jsonMember.receivedSkillLog.Add(jsonSkill);
                }

                var abnormals = exdata.Abnormals.Get(player);
                foreach (var abnormal in abnormals.Times) {
                    foreach (var duration in abnormal.Value.AllDurations()) {
                        jsonMember.abnormals.Add(new JsonAbnormal() {
                            id = abnormal.Key.Id,
                            start = (int) ((duration.Begin - exdata.FirstTick) / TimeSpan.TicksPerMillisecond),
                            end = (int) ((duration.End - exdata.FirstTick) / TimeSpan.TicksPerMillisecond),
                            stack = duration.Stack
                        });
                    }
                }

                jsonData.players.Add(jsonMember);
            }

            Task.Run(() => {
                lock (savelock) //don't save 2 files at one time
                {
                    if (file.Exists) { return; } //double check if file was created while we were preparing export in other thread

                    var libpath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, Environment.Is64BitProcess ? "lib/7z_x64.dll" : "lib/7z.dll");
                    SevenZipBase.SetLibraryPath(libpath);
                    var compressor = new SevenZipCompressor {ArchiveFormat = OutArchiveFormat.SevenZip};
                    compressor.CustomParameters["tc"] = "off";
                    compressor.CompressionLevel = CompressionLevel.Ultra;
                    compressor.CompressionMode = CompressionMode.Create;
                    compressor.TempFolderPath = Path.GetTempPath();
                    compressor.PreserveDirectoryRoot = false;
                    compressor.DefaultItemName = Path.GetFileNameWithoutExtension(fileName) + ".json";
                    using MemoryStream s = new MemoryStream();
                    using StreamWriter w = new StreamWriter(s);
                    using JsonTextWriter jsonw = new JsonTextWriter(w);
                    JsonSerializer ser = new JsonSerializer();
                    ser.NullValueHandling = NullValueHandling.Ignore;
                    ser.Formatting = Formatting.Indented;
                    ser.Serialize(jsonw, jsonData);
                    jsonw.Flush();
                    s.Position = 0;
                    using var cs = File.Create(fname);
                    compressor.CompressStream(s, cs);
                }
            });
        }
    }
}
