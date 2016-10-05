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
            Connect();
        }

        private void Connect()
        {
            _client = new DiscordClient();

            //Convert our sync method to an async one and block the Main function until the bot disconnects

            _client.Ready += _client_Ready;
            try
            {
                _client.Connect(BasicTeraData.Instance.WindowData.DiscordLogin, BasicTeraData.Instance.WindowData.DiscordPassword);
            }
            catch (Exception e)
            {

                //Failing here is not a reason to make the meter crash
                BasicTeraData.LogError(e.Message + "\n" + e.StackTrace + "\n" + e.InnerException + "\n" + e, false, false);
                return;
            }
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
            if (!_ready)
            {
                Connect();
            }
            try
            {
                if (removeAll)
                {
                    var messages = await _client.GetServer(server).GetChannel(channel).DownloadMessages();
                    foreach (var msg in messages)
                    {
                        if (msg.IsAuthor)
                        {
                            if (msg.Timestamp > DateTime.Now.AddSeconds(-30))
                            {
                                //Don't send too much msg
                                return;
                            }
                            await msg.Delete();
                        }
                    }
                }
                await _client.GetServer(server).GetChannel(channel).SendMessage(message);
            }catch(Exception e)
            {
                //Failing here is not a reason to make the meter crash
                BasicTeraData.LogError(e.Message + "\n" + e.StackTrace + "\n" + e.InnerException + "\n" + e, false, false);
            }
        }

        private static Discord _instance;


        public static Discord Instance => _instance ?? (_instance = new Discord());


    }
}
