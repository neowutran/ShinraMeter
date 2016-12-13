using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Text;
using System.Linq;
using System.Text;
using System.Threading;
using DamageMeter.Database.Structures;
using Data;
using Lang;
using Tera.Game.Abnormality;
using Clipboard = System.Windows.Clipboard;

namespace DamageMeter
{
    public static class CopyPaste
    {
        private static readonly object Lock = new object();
        internal static PrivateFontCollection PFC = new PrivateFontCollection();
        internal static Font Font=new Font("Trebuchet MS", 12, FontStyle.Bold, GraphicsUnit.Pixel);
        private static Graphics graphics = Graphics.FromImage(new Bitmap(1, 1));
        public static void Paste(string text)
        {
            if (!Monitor.TryEnter(Lock)) return;
            TeraWindow.SendString(text);
            Monitor.Exit(Lock);
        }

        public static void CopyInspect(string name)
        {

            var clip = string.Empty;
            if (NetworkController.Instance.Server.Region == "TW")
                clip = "/查看 ";
            else if (NetworkController.Instance.Server.Region == "JP")
                clip = "/詳細確認 ";
            else if (NetworkController.Instance.Server.Region == "KR")
                clip = "/살펴보기 ";
            else
                clip = "/inspect ";

            for (var i = 0; i < 3; i++)
            {
                try
                {
                    Clipboard.SetText(clip + name);
                    break;
                }
                catch
                {
                    Thread.Sleep(100);
                    //Ignore
                }
            }
        }


        public static Tuple<string,string> Copy(StatsSummary statsSummary, Skills skills, AbnormalityStorage abnormals,
            bool timedEncounter, string header, string content,
            string footer, string orderby, string order, string lowDpsContent, int lowDpsThreshold)
        {
            //stop if nothing to paste
            var entityInfo = statsSummary.EntityInformation;
            var playersInfos = statsSummary.PlayerDamageDealt;
            var firstTick = entityInfo.BeginTime;
            var lastTick = entityInfo.EndTime;
            var firstHit = firstTick/TimeSpan.TicksPerSecond;
            var lastHit = lastTick/TimeSpan.TicksPerSecond;
            var heals = statsSummary.PlayerHealDealt;
            playersInfos.RemoveAll(x => x.Amount == 0);
            
            IEnumerable<PlayerDamageDealt> playerInfosOrdered;
            if (order == "ascending")
            {
                switch (orderby)
                {
                    case "damage_received":
                        playerInfosOrdered =
                            playersInfos.OrderBy(
                                playerInfo =>
                                    skills.DamageReceived(playerInfo.Source.User, entityInfo.Entity,
                                        timedEncounter));
                        break;
                    case "name":
                        playerInfosOrdered = playersInfos.OrderBy(playerInfo => playerInfo.Source.Name);
                        break;
                    case "damage_percentage":
                    case "damage_dealt":
                    case "dps":
                        playerInfosOrdered = playersInfos.OrderBy(playerInfo => playerInfo.Amount);
                        break;
                    case "crit_rate":
                        playerInfosOrdered = playersInfos.OrderBy(playerInfo => playerInfo.CritRate);
                        break;
                    case "hits_received":
                        playerInfosOrdered =
                            playersInfos.OrderBy(
                                playerInfo =>
                                    skills.HitsReceived(playerInfo.Source.User, entityInfo.Entity, timedEncounter));
                        break;
                    default:
                        Console.WriteLine("wrong value for orderby");
                        throw new Exception("wrong value for orderby");
                }
            }
            else
            {
                switch (orderby)
                {
                    case "damage_received":
                        playerInfosOrdered =
                            playersInfos.OrderByDescending(
                                playerInfo =>
                                    skills.DamageReceived(playerInfo.Source.User, entityInfo.Entity,
                                        timedEncounter));
                        break;
                    case "name":
                        playerInfosOrdered = playersInfos.OrderByDescending(playerInfo => playerInfo.Source.Name);
                        break;
                    case "damage_percentage":
                    case "damage_dealt":
                    case "dps":
                        playerInfosOrdered = playersInfos.OrderByDescending(playerInfo => playerInfo.Amount);
                        break;
                    case "crit_rate":
                        playerInfosOrdered = playersInfos.OrderByDescending(playerInfo => playerInfo.CritRate);
                        break;
                    case "hits_received":
                        playerInfosOrdered =
                            playersInfos.OrderByDescending(
                                playerInfo =>
                                    skills.HitsReceived(playerInfo.Source.User, entityInfo.Entity, timedEncounter));
                        break;
                    default:
                        Console.WriteLine("wrong value for orderby");
                        throw new Exception("wrong value for orderby");
                }
            }

            var dpsString = new StringBuilder(header);

            var name = entityInfo.Entity?.Info.Name ?? "";
            AbnormalityDuration enrage;
            var bossDebuff = abnormals.Get(entityInfo.Entity);
            bossDebuff.TryGetValue(BasicTeraData.Instance.HotDotDatabase.Enraged, out enrage);
            var enrageperc = lastTick - firstTick == 0
                ? 0
                : (double) (enrage?.Duration(firstTick, lastTick) ?? 0)/(lastTick - firstTick);

            dpsString.Replace("{encounter}", name);
            var interval = TimeSpan.FromSeconds(lastHit - firstHit);
            dpsString.Replace("{timer}", interval.ToString(@"mm\:ss"));
            dpsString.Replace("{partyDps}",
                FormatHelpers.Instance.FormatValue(lastHit - firstHit > 0
                    ? entityInfo.TotalDamage/(lastHit - firstHit)
                    : 0) + LP.PerSecond);
            dpsString.Replace("{enrage}", FormatHelpers.Instance.FormatPercent(enrageperc));
            dpsString.Replace("{debuff_list}", String.Join(" | ",
                bossDebuff.Where(x => x.Key.Id != 8888888 && x.Value.Duration(firstTick, lastTick) > 0).ToList().Select(
                    x => x.Key.ShortName + " " + FormatHelpers.Instance.FormatPercent((double)x.Value.Duration(firstTick, lastTick) / (lastTick - firstTick)) +
                        " (" + TimeSpan.FromTicks(x.Value.Duration(firstTick, lastTick)).ToString(@"mm\:ss") + ") ")
            ));
            dpsString.Replace("{debuff_list_p}", String.Join(" | ",
                bossDebuff.Where(x => x.Key.Id != 8888888 && x.Value.Duration(firstTick, lastTick) > 0).ToList().Select(
                    x => x.Key.ShortName + " " + FormatHelpers.Instance.FormatPercent((double)x.Value.Duration(firstTick, lastTick) / (lastTick - firstTick)))
            ));

            var placeholders = new List<KeyValuePair<PlayerDamageDealt, Dictionary<string, string>>>();
            foreach (var playerStats in playerInfosOrdered)
            {
                var playerHolder = new Dictionary<string, string>();
                placeholders.Add(new KeyValuePair<PlayerDamageDealt, Dictionary<string, string>>(playerStats, playerHolder));
                var buffs = abnormals.Get(playerStats.Source);
                AbnormalityDuration slaying;
                var firstOrDefault = heals.FirstOrDefault(x => x.Source == playerStats.Source);
                double healCritrate = 0;
                if (firstOrDefault != null)
                {
                    healCritrate = firstOrDefault.CritRate;
                }
                buffs.Times.TryGetValue(BasicTeraData.Instance.HotDotDatabase.Slaying, out slaying);
                var slayingperc = lastTick - firstTick == 0
                    ? 0
                    : (double)(slaying?.Duration(firstTick, lastTick) ?? 0) / (lastTick - firstTick);
                playerHolder["{slaying}"] = FormatHelpers.Instance.FormatPercent(slayingperc);
                playerHolder["{dps}"] = FormatHelpers.Instance.FormatValue(playerStats.Interval == 0 ? playerStats.Amount : playerStats.Amount * TimeSpan.TicksPerSecond / playerStats.Interval) + LP.PerSecond;
                playerHolder["{global_dps}"] = FormatHelpers.Instance.FormatValue(entityInfo.Interval == 0 ? playerStats.Amount : playerStats.Amount * TimeSpan.TicksPerSecond / entityInfo.Interval) + LP.PerSecond;
                playerHolder["{interval}"] = playerStats.Interval/TimeSpan.TicksPerSecond + LP.Seconds;
                playerHolder["{damage_dealt}"] = FormatHelpers.Instance.FormatValue(playerStats.Amount);
                playerHolder["{class}"] = LP.ResourceManager.GetString(playerStats.Source.Class.ToString(), LP.Culture) + "";
                playerHolder["{fullname}"] = playerStats.Source.FullName;
                playerHolder["{name}"] = playerStats.Source.Name;
                playerHolder["{deaths}"] = buffs.Death.Count(firstTick, lastTick) + "";
                playerHolder["{death_duration}"] = TimeSpan.FromTicks(buffs.Death.Duration(firstTick, lastTick)).ToString(@"mm\:ss");
                playerHolder["{aggro}"] = buffs.Aggro(entityInfo.Entity).Count(firstTick, lastTick) + "";
                playerHolder["{aggro_duration}"] = TimeSpan.FromTicks(buffs.Aggro(entityInfo.Entity).Duration(firstTick, lastTick)).ToString(@"mm\:ss");
                playerHolder["{damage_percentage}"] = playerStats.Amount * 100 / entityInfo.TotalDamage + "%";
                playerHolder["{crit_rate}"] = playerStats.CritRate + "%";
                playerHolder["{crit_rate_heal}"] = healCritrate + "%";
                playerHolder["{biggest_crit}"] = FormatHelpers.Instance.FormatValue(skills.BiggestCrit(playerStats.Source.User, entityInfo.Entity, timedEncounter));
                playerHolder["{damage_received}"] = FormatHelpers.Instance.FormatValue(skills.DamageReceived(playerStats.Source.User, entityInfo.Entity, timedEncounter));
                playerHolder["{hits_received}"] = FormatHelpers.Instance.FormatValue(skills.HitsReceived(playerStats.Source.User, entityInfo.Entity, timedEncounter));
                playerHolder["{debuff_list}"] = String.Join(" | ",
                    bossDebuff.Where(x=>x.Key.Id!=8888888 && x.Value.InitialPlayerClass==playerStats.Source.Class && x.Value.Duration(firstTick,lastTick)>0).ToList().Select(
                        x=>x.Key.ShortName + " " + FormatHelpers.Instance.FormatPercent((double)x.Value.Duration(firstTick,lastTick) / (lastTick - firstTick)) +
                            " ("+ TimeSpan.FromTicks(x.Value.Duration(firstTick, lastTick)).ToString(@"mm\:ss")+") ")
                );
                playerHolder["{debuff_list_p}"] = String.Join(" | ",
                    bossDebuff.Where(x => x.Key.Id != 8888888 && x.Value.InitialPlayerClass == playerStats.Source.Class && x.Value.Duration(firstTick, lastTick) > 0).ToList().Select(
                        x => x.Key.ShortName + " " + FormatHelpers.Instance.FormatPercent((double)x.Value.Duration(firstTick, lastTick) / (lastTick - firstTick)))
                );
            }
            var placeholderLength = placeholders.SelectMany(x => x.Value).GroupBy(x=>x.Key).ToDictionary(x=>x.Key,x=>x.Max(z=> graphics.MeasureString(z.Value, Font, default(PointF), StringFormat.GenericTypographic).Width));
            var dpsmono = new StringBuilder(dpsString.ToString());
            var placeholderMono = placeholders.SelectMany(x => x.Value).GroupBy(x => x.Key).ToDictionary(x => x.Key, x => x.Max(z => z.Value.Length));
            if ((content.Contains('\\')||lowDpsContent.Contains('\\')) && BasicTeraData.Instance.WindowData.FormatPasteString)
                placeholders.ForEach(x =>
                {
                    var currentContent = x.Key.Amount*100/entityInfo.TotalDamage >= lowDpsThreshold ? new StringBuilder(content): new StringBuilder(lowDpsContent);
                    x.Value.ToList().ForEach(z => currentContent.Replace(z.Key, PadRight(z.Value,placeholderLength[z.Key])));
                    dpsString.Append(currentContent);
                    currentContent = x.Key.Amount * 100 / entityInfo.TotalDamage >= lowDpsThreshold ? new StringBuilder(content) : new StringBuilder(lowDpsContent);
                    x.Value.ToList().ForEach(z => currentContent.Replace(z.Key, z.Value.PadRight(placeholderMono[z.Key])));
                    dpsmono.Append(currentContent);
                });
            else
                { placeholders.ForEach(x =>
                    {
                        var currentContent = x.Key.Amount * 100 / entityInfo.TotalDamage >= lowDpsThreshold ? new StringBuilder(content) : new StringBuilder(lowDpsContent);
                        x.Value.ToList().ForEach(z => currentContent.Replace(z.Key, z.Value));
                        dpsString.Append(currentContent);
                    });
                dpsmono = dpsString;
                }
            var footerstr=footer.Replace("{debuff_list}", String.Join(" | ",
                    bossDebuff.Where(x => x.Key.Id != 8888888 && x.Value.Duration(firstTick, lastTick) > 0).ToList().Select(
                        x => x.Key.ShortName + " " + FormatHelpers.Instance.FormatPercent((double)x.Value.Duration(firstTick, lastTick) / (lastTick - firstTick)) +
                        " (" + TimeSpan.FromTicks(x.Value.Duration(firstTick, lastTick)).ToString(@"mm\:ss") + ") ")
                )).Replace("{debuff_list_p}", String.Join(" | ",
                    bossDebuff.Where(x => x.Key.Id != 8888888 && x.Value.Duration(firstTick, lastTick) > 0).ToList().Select(
                        x => x.Key.ShortName + " " + FormatHelpers.Instance.FormatPercent((double)x.Value.Duration(firstTick, lastTick) / (lastTick - firstTick)))
                ));
            dpsString.Append(footerstr);
            dpsmono.Append(footerstr);
            var paste = dpsString.ToString();
            var monoPaste = dpsmono.ToString();
            while (paste.Contains(" \\")) paste = paste.Replace(" \\", "\\");
            while (monoPaste.Contains(" \\")) monoPaste = monoPaste.Replace(" \\", "\\");
            monoPaste = monoPaste.Replace("\\", Environment.NewLine);
            return new Tuple<string,string>(paste ,monoPaste);
        }

        private static string PadRight(string str, double length)
        {
            var result = string.IsNullOrWhiteSpace(str)&&length>0?"-":str;
            var test = string.IsNullOrWhiteSpace(str) && length > 0 ? "-" : str;
            var olddelta = length - graphics.MeasureString(result, Font, default(PointF), StringFormat.GenericTypographic).Width;
            var delta = length - graphics.MeasureString(result, Font, default(PointF), StringFormat.GenericTypographic).Width;
            while (delta > 0)
            {
                test = " " + test;
                result = result + " ";
                olddelta = delta;
                delta = length - graphics.MeasureString(test, Font, default(PointF), StringFormat.GenericTypographic).Width;
            }
            return olddelta+delta>=0?result:result.EndsWith(" ")?result.Substring(0,result.Length-1):result;
        }
    }
}