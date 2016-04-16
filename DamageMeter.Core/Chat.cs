using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
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
            if(_chat.Count == _maxMessage)
            {
                _chat.RemoveFirst();
            }

            ChatMessage chatMessage = new ChatMessage(message.Username, message.Text);
            _chat.AddLast(chatMessage);

        }

        public List<ChatMessage> Get()
        {
            return _chat.ToList();
        }
  
        public static Chat Instance => _instance ?? (_instance = new Chat());

    }
}
