using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text.RegularExpressions;
using Tera.Game.Messages;

namespace DamageMeter
{
    public class Chat
    {
        private static Chat _instance;

        private readonly LinkedList<ChatMessage> _chat = new LinkedList<ChatMessage>();
        private readonly int _maxMessage = 100;

        public enum ChatType
        {
            Whisper = 0,
            Normal = 1
        }


        private Chat()
        {
        }

        public static Chat Instance => _instance ?? (_instance = new Chat());

        public void Add(S_CHAT message)
        {
            Add(message.Username, message.Text, ChatType.Normal);
        }

        public void Add(S_WHISPER message)
        {
            Add(message.Sender, message.Text, ChatType.Whisper);
        }

        private void Add(string sender, string message, ChatType chatType)
        {
            if (_chat.Count == _maxMessage)
            {
                _chat.RemoveFirst();
            }

            var rgx = new Regex("<[^>]+>");
            message = rgx.Replace(message, "");
            message = WebUtility.HtmlDecode(message);

            if(chatType == ChatType.Whisper && NetworkController.Instance.EntityTracker.MeterUser.Name != sender && !TeraWindow.IsTeraActive())
            {
                NetworkController.Instance.FlashMessage = new System.Tuple<string, string>("Whisper: "+sender, message);
            }

            if (chatType == ChatType.Normal && NetworkController.Instance.EntityTracker.MeterUser.Name != sender && !TeraWindow.IsTeraActive() && message.Contains("@"+ NetworkController.Instance.EntityTracker.MeterUser.Name))
            {
                NetworkController.Instance.FlashMessage = new System.Tuple<string, string>("Chat: " + sender, message);
            }

            var chatMessage = new ChatMessage(sender, message, chatType);
            _chat.AddLast(chatMessage);
        }

        public List<ChatMessage> Get()
        {
            return _chat.ToList();
        }
    }
}