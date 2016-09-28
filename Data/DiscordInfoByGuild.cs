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
        public DiscordInfoByGuild(ulong discordServer, ulong discordChannelGuildInfo, ulong discordChannelGuildQuest)
        {
            DiscordServer = discordServer;
            DiscordChannelGuildQuest = discordChannelGuildQuest;
            DiscordChannelGuildInfo = discordChannelGuildInfo;
        }
    }
}
