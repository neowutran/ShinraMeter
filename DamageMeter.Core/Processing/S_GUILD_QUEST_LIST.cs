using Data;
using Lang;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Tera.Game;

namespace DamageMeter.Processing
{
    internal class S_GUILD_QUEST_LIST
    {

        private static string ReplaceGuildInfo(string str, Tera.Game.Messages.S_GUILD_QUEST_LIST guildquest, DiscordInfoByGuild discordInfo)
        {
            str = str.Replace("{guild_guildname}", guildquest.GuildName);
            str = str.Replace("{guild_gold}", guildquest.Gold.ToString());
            str = str.Replace("{guild_creationtime}", guildquest.GuildCreationTime.ToString(@"yyyy-MM-dd"));
            str = str.Replace("{guild_lvl}", guildquest.GuildLevel.ToString());
            str = str.Replace("{guild_master}", guildquest.GuildMaster);
            str = str.Replace("{guild_size}", guildquest.GuildSize.ToString());
            str = str.Replace("{guild_xp_to_next_level}", (guildquest.GuildXpNextLevel - guildquest.GuildXpCurrent).ToString());
            str = str.Replace("{guild_current_xp}", guildquest.GuildXpCurrent.ToString());
            str = str.Replace("{guild_next_level_xp}", guildquest.GuildXpNextLevel.ToString());
            str = str.Replace("{guild_number_accounts}", guildquest.NumberAccount.ToString());
            str = str.Replace("{guild_number_characters}", guildquest.NumberCharacters.ToString());
            str = str.Replace("{guild_number_quest_done}", guildquest.NumberQuestsDone.ToString());
            str = str.Replace("{guild_total_number_quest}", guildquest.NumberTotalDailyQuest.ToString());
            str = str.Replace("{guild_number_quest_remaining}", (guildquest.NumberTotalDailyQuest - guildquest.NumberQuestsDone).ToString());
            str = str.Replace("{gold_label}", BasicTeraData.Instance.QuestInfoDatabase.Get(20000000));
            str = str.Replace("{xp_label}", BasicTeraData.Instance.QuestInfoDatabase.Get(20000001));
            var activeQuest = ReplaceNoQuest(discordInfo.QuestNoActiveText);
            var quest = guildquest.ActiveQuest();
            if(quest != null)
            {
                activeQuest = discordInfo.QuestInfoText;
                activeQuest = ReplaceQuestInfo(activeQuest, quest, discordInfo);
            }
            str = str.Replace("{active_quest}", activeQuest);
            return str;
        }

        private static string ReplaceNoQuest(string str)
        {
            str = str.Replace("{no_quest_text}", LP.No_active_quest);
            return str;
        }

        private static string ReplaceQuestInfo(string str, GuildQuest quest, DiscordInfoByGuild discordInfo)
        {
            str = str.Replace("{quest_guildname}", quest.GuildName);
            str = str.Replace("{quest_type}", quest.GuildQuestType1.ToString());
            str = str.Replace("{quest_size}", quest.QuestSize.ToString());
            str = str.Replace("{quest_time_remaining}", quest.TimeRemaining.ToString(@"hh\:mm\:ss"));
            var isBamQuest = false;
            foreach(var target in quest.Targets)
            {
                if(target.TotalQuest == 1)
                {
                    isBamQuest = true;
                }
            }

            str = str.Replace("{quest_is_bam_quest}", isBamQuest.ToString());

            var rewardStr = discordInfo.RewardHeaderText;
            foreach(var reward in quest.Rewards)
            {
                rewardStr += ReplaceRewardInfo(discordInfo.RewardContentText, reward);
            }            
            rewardStr += discordInfo.RewardFooterText;
            str = str.Replace("{rewards}", rewardStr);

            var targetStr = discordInfo.TargetHeaderText;
            foreach (var target in quest.Targets)
            {
                targetStr += ReplaceTargetInfo(discordInfo.TargetContentText, quest, target);
            }
            targetStr += discordInfo.TargetFooterText;
            str = str.Replace("{targets}", targetStr);

            return str;
        }

        private static string ReplaceRewardInfo(string str, GuildQuestItem reward)
        {
            str = str.Replace("{reward_name}", BasicTeraData.Instance.QuestInfoDatabase.Get((int)reward.ItemId));
            str = str.Replace("{reward_amount}", reward.Amount.ToString());

            return str;
        }

        private static string ReplaceTargetInfo(string str, GuildQuest quest, GuildQuestTarget target)
        {
            str = str.Replace("{target_current_count}", target.CountQuest.ToString());
            str = str.Replace("{target_total_count}", target.TotalQuest.ToString());
            str = str.Replace("{target_remaining}", (target.TotalQuest - target.CountQuest).ToString());
            var targetName = "";
            switch (quest.GuildQuestType1)
            {
                case Tera.Game.Messages.S_GUILD_QUEST_LIST.GuildQuestType.Hunt:
                    targetName = BasicTeraData.Instance.MonsterDatabase.GetAreaName((ushort)target.ZoneId);
                    break;
                case Tera.Game.Messages.S_GUILD_QUEST_LIST.GuildQuestType.Battleground:
                case Tera.Game.Messages.S_GUILD_QUEST_LIST.GuildQuestType.Gathering:
                    targetName += BasicTeraData.Instance.QuestInfoDatabase.Get((int)target.TargetId);
                    break;
            }
            str = str.Replace("{target_name}", targetName);
            return str;

        }
     
       internal S_GUILD_QUEST_LIST(Tera.Game.Messages.S_GUILD_QUEST_LIST guildquest)
        {
            if (BasicTeraData.Instance.WindowData.DiscordLogin == "") return;
            DiscordInfoByGuild discordData = null;

            var guildname = (NetworkController.Instance.Server.Name.ToLowerInvariant() + "_" + guildquest.GuildName.ToLowerInvariant()).Replace(" ", "");
            BasicTeraData.Instance.WindowData.DiscordInfoByGuild.TryGetValue(guildname, out discordData);

            if (discordData == null) return;
            var quest = guildquest.ActiveQuest();
            if (quest == null)
            {
                var activeQuestThread = new Thread(() => Discord.Instance.Send(discordData.DiscordServer, discordData.DiscordChannelGuildQuest, ReplaceGuildInfo(ReplaceNoQuest(discordData.QuestNoActiveText), guildquest, discordData), true));
                activeQuestThread.Start();
            }
            else
            {
                var activeQuestThread = new Thread(() => Discord.Instance.Send(discordData.DiscordServer, discordData.DiscordChannelGuildQuest, ReplaceGuildInfo(ReplaceQuestInfo(discordData.QuestInfoText, quest, discordData), guildquest, discordData), true));
                activeQuestThread.Start();
            }
            var thread = new Thread(() => Discord.Instance.Send(discordData.DiscordServer, discordData.DiscordChannelGuildInfo, ReplaceGuildInfo(discordData.GuildInfosText, guildquest, discordData), true));
            thread.Start();            
        }
    }
}
