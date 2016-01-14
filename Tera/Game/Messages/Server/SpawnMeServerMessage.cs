using System;

namespace Tera.Game.Messages
{
    public class SpawnMeServerMessage : ParsedMessage
    {
        internal SpawnMeServerMessage(TeraMessageReader reader)
            : base(reader)
        {
            Id = reader.ReadEntityId();
            
         //   PrintRaw();
         //   Console.WriteLine(reader.ReadTeraString());
          //  Console.WriteLine(reader.ReadTeraString());
            //Console.WriteLine(reader.ReadInt32());
            //Console.WriteLine(reader.ReadInt32());
        }

        public EntityId Id { get; private set; }
    }
}