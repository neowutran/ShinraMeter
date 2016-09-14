using Tera.Game.Messages;

namespace DamageMeter
{
    public class ChatMessage
    {
        public ChatMessage(string sender, string text, Chat.ChatType chatType, S_CHAT.ChannelEnum? channel, string time)
        {
            Text = text;
            Sender = sender;
            ChatType = chatType;
            Channel = channel;
            Time = time;
        }

        public Chat.ChatType ChatType { private set; get; }
        public string Time { get; set; }

        public S_CHAT.ChannelEnum? Channel { private set; get; }

        public string Sender { private set; get; }
        public string Text { private set; get; }
    }
}