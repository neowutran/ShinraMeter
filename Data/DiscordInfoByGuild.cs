using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Data
{
    public class DiscordInfoByGuild
    {
        public ulong DiscordServer { get; set; }
        public ulong DiscordChannelGuildInfo { get; set; }
        public ulong DiscordChannelGuildQuest { get; set; }

        public string GuildInfosText { get; private set; }
        public string QuestInfoText { get; private set; }
        public string QuestListInfoText { get; private set; }
        public string QuestListHeaderText { get; private set; }
        public string RewardContentText { get; private set; }
        public string RewardFooterText { get; private set; }
        public string RewardHeaderText { get; private set; }
        public string TargetHeaderText { get; private set; }
        public string TargetContentText { get; private set; }
        public string TargetFooterText { get; private set; }
        public string QuestNoActiveText { get; private set; }

        public DiscordInfoByGuild(
            ulong discordServer, 
            ulong discordChannelGuildInfo, 
            ulong discordChannelGuildQuest,
            string guildInfosText,
            string questInfoText,
            string questListInfoText,
            string questListHeaderText,
            string rewardFooterText,
            string rewardContentText,
            string rewardHeaderText,
            string targetHeaderText,
            string targetContentText,
            string targetFooterText,
            string questNoActiveText
            )
        {
            DiscordServer = discordServer;
            DiscordChannelGuildQuest = discordChannelGuildQuest;
            DiscordChannelGuildInfo = discordChannelGuildInfo;
            GuildInfosText = guildInfosText;
            QuestInfoText = questInfoText;
            QuestListInfoText = questListInfoText;
            QuestListHeaderText = questListHeaderText;
            RewardFooterText = rewardFooterText;
            RewardContentText =  rewardContentText;
            RewardHeaderText = rewardHeaderText;
            TargetHeaderText = targetHeaderText;
            TargetContentText =  targetContentText;
            TargetFooterText = targetFooterText;
            QuestNoActiveText = questNoActiveText;
        }
    }
}
