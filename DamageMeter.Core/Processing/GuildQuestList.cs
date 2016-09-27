using Data;
using Lang;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Tera.Game.Messages;

namespace DamageMeter.Processing
{
    public static class GuildQuestList
    {
     
       public static void Process(S_GUILD_QUEST_LIST guildquest)
        {
            if (BasicTeraData.Instance.WindowData.DiscordLogin == "") return;
            DiscordInfoByGuild discordData = null;
            var guildname = NetworkController.Instance.Server.Name.ToLowerInvariant() + "_" + guildquest.GuildName.ToLowerInvariant();
            BasicTeraData.Instance.WindowData.DiscordInfoByGuild.TryGetValue(guildname, out discordData);

            if (discordData == null) return;
            var activeQuest = guildquest.ActiveQuest();
            if (activeQuest != null)
            {
                string targets = "";
                switch (activeQuest.GuildQuestType1)
                {
                    case S_GUILD_QUEST_LIST.GuildQuestType.Hunt:
                        foreach (var target in activeQuest.Targets)
                        {
                            targets += BasicTeraData.Instance.MonsterDatabase.GetAreaName((ushort)target.ZoneId) + ": " + target.CountQuest + "/" + target.TotalQuest + "\n";

                        }
                        break;
                    case S_GUILD_QUEST_LIST.GuildQuestType.Battleground:
                    case S_GUILD_QUEST_LIST.GuildQuestType.Gathering:
                        foreach (var target in activeQuest.Targets)
                        {
                            targets += BasicTeraData.Instance.QuestInfoDatabase.Get((int)target.TargetId) + ": " + target.CountQuest + "/" + target.TotalQuest + "\n";
                        }
                        break;
                }

                var activeQuestStr = ":dart:" + guildquest.GuildName + ":dart:\n";
                activeQuestStr += "\n" + activeQuest.QuestSize + " " + activeQuest.GuildQuestType1 + " (" + activeQuest.GuildQuestType2 + ") quest, activated by the guild " + activeQuest.GuildName;
                activeQuestStr += "\nRemaining time: " + activeQuest.TimeRemaining.ToString(@"hh\:mm\:ss");
                activeQuestStr += "\n" + targets;
                var activeQuestThread = new Thread(() => Discord.Instance.Send(discordData.DiscordServer, discordData.DiscordChannelGuildQuest, activeQuestStr, true));
                activeQuestThread.Start();
            }
            else
            {
                var activeQuestStr = ":dart:" + guildquest.GuildName + ":dart: \n\n";
                activeQuestStr += LP.No_active_quest;
                var activeQuestThread = new Thread(() => Discord.Instance.Send(discordData.DiscordServer, discordData.DiscordChannelGuildQuest, activeQuestStr, true));
                activeQuestThread.Start();
            }

            var guildStr = ":dart: " + guildquest.GuildName + " :dart: \n\n";
            guildStr += "lvl " + guildquest.GuildLevel + "\n" + guildquest.GuildMaster + " - " + guildquest.GuildSize + "\n";
            guildStr += BasicTeraData.Instance.QuestInfoDatabase.Get(20000000) + ": " + guildquest.Gold + "\n";
            guildStr += "XP for next lvl: " + (guildquest.GuildXpNextLevel - guildquest.GuildXpCurrent) + "\n";
            guildStr += "Creation time: " + guildquest.GuildCreationTime.ToString(@"yyyy-mm-dd") + "\n";
            guildStr += LP.Quests_status + guildquest.NumberQuestsDone + "/" + guildquest.NumberTotalDailyQuest;
            var thread = new Thread(() => Discord.Instance.Send(discordData.DiscordServer, discordData.DiscordChannelGuildInfo, guildStr, true));
            thread.Start();
        }
    }
}
