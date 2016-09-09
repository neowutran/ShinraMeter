namespace DamageMeter
{
    public class ChatMessage
    {
        public ChatMessage(string sender, string text, Chat.ChatType chatType)
        {
            Text = text;
            Sender = sender;
            ChatType = chatType;
        }

        public Chat.ChatType ChatType { private set; get; }

        public string Sender { private set; get; }
        public string Text { private set; get; }
    }
}