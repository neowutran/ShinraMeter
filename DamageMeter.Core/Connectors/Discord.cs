using Data;
using Discord;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace DamageMeter
{
    public class Discord
    {

        private DiscordClient _client;
        private bool _ready = false;
        private Discord()
        {
            _client = new DiscordClient();

            //Convert our sync method to an async one and block the Main function until the bot disconnects

            _client.Ready += _client_Ready;
            Console.WriteLine(BasicTeraData.Instance.WindowData.DiscordLogin + ":" + BasicTeraData.Instance.WindowData.DiscordPassword);
            _client.Connect(BasicTeraData.Instance.WindowData.DiscordLogin, BasicTeraData.Instance.WindowData.DiscordPassword);
            while (!_ready)
            {
                Thread.Sleep(1000);
            }
        }

        private void _client_Ready(object sender, EventArgs e)
        {
            _ready = true;
        }

        public async void Send(ulong server, ulong channel,string message, bool removeAll)
        {
            if (removeAll)
            {    
                var messages = await _client.GetServer(server).GetChannel(channel).DownloadMessages();
                foreach(var msg in messages)
                {
                    if (msg.IsAuthor)
                    {
                        if(msg.Timestamp > DateTime.Now.AddSeconds(-30))
                        {
                            //Don't send too much msg
                            return;
                        }
                        await msg.Delete();
                    }
                }
            }
            await _client.GetServer(server).GetChannel(channel).SendMessage(message);
        }

        private static Discord _instance;


        public static Discord Instance => _instance ?? (_instance = new Discord());


    }
}
