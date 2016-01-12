using System;

namespace Tera.Game.Messages
{
    public class CChat : ParsedMessage
    {
        internal CChat(TeraMessageReader reader) : base(reader)
        {
            //    reader.Skip(2);
            Canal = reader.ReadBytes(6);

            Console.WriteLine("Canal:" + BitConverter.ToString(Canal));

            Text = reader.ReadTeraString();
            
            Console.WriteLine("text:" + Text);
        }

        public string Text { get; set; }

        public byte[] Canal { get; set; }
    }
}