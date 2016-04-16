using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Tera.Game.Messages;

namespace DamageMeter
{
    public class Chat
    {
        private static Chat _instance;

    
        private Chat()
        {
        }

        private LinkedList<ChatMessage> _chat = new LinkedList<ChatMessage>();
        private int _maxMessage = 100;

        public void Add(S_CHAT message)
        {
            Add(message.Username, message.Text);
        }

        public void Add(S_WHISPER message)
        {
            Add(message.Sender, message.Text);
        }

        private void Add(string sender, string message)
        {
            if (_chat.Count == _maxMessage)
            {
                _chat.RemoveFirst();
            }

            Regex rgx = new Regex("<[^>]+>");
            message = rgx.Replace(message, "");
            message = WebUtility.HtmlDecode(message);

            ChatMessage chatMessage = new ChatMessage(sender, message);
            _chat.AddLast(chatMessage);
        }

        public List<ChatMessage> Get()
        {
            return _chat.ToList();
        }
  
        public static Chat Instance => _instance ?? (_instance = new Chat());

    }
}
