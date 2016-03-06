using System;

namespace Tera.Game.Messages
{
    public class LoginServerMessage : ParsedMessage
    {
        internal LoginServerMessage(TeraMessageReader reader)
            : base(reader)
        {
            reader.Skip(10);
            RaceGenderClass = new RaceGenderClass(reader.ReadInt32());
            Id = reader.ReadEntityId();
            reader.Skip(4);
            PlayerId = reader.ReadUInt32();


            reader.Skip(272);

            /* 
            // Unless now, every region have switched to the new loginServerMessage structure
            reader.Skip(220);

            var nameFirstBit = false;
            while (true)
            {
                var b = reader.ReadByte();
                if (b == 0x80)
                {
                    nameFirstBit = true;
                    continue;
                }
                if (b == 0x3F && nameFirstBit)
                {
                    break;
                }
                nameFirstBit = false;
            }

            reader.Skip(9);
            */

            Name = reader.ReadTeraString();
          
        }

        public EntityId Id { get; private set; }
        public uint PlayerId { get; private set; }
        public string Name { get; private set; }
        public string GuildName { get; private set; }

        public PlayerClass Class => RaceGenderClass.Class;

        public RaceGenderClass RaceGenderClass { get; }
    }
}