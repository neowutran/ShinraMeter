using System;

namespace Tera.Game.Messages
{
    public class EachSkillResultServerMessage : ParsedMessage
    {
        [Flags]
        public enum SkillResultFlags
        {
            Bit0 = 1, // Usually 1 for attacks, 0 for blocks/dodges but I don't understand its exact semantics yet
            Heal = 2, // Bit0 == 1 + heal == 1 = mana
            Bit2 = 4,
            Bit16 = 0x10000,
            Bit18 = 0x40000
        }

        internal EachSkillResultServerMessage(TeraMessageReader reader)
            : base(reader)
        {
            reader.Skip(4);
            Source = reader.ReadEntityId();
            Target = reader.ReadEntityId();

           

            //I think it s some kind of source ID.
            //When I use a skill against any monstrer, it s always the same value
            //When I pick up a mana mote, differente ID
            Unknow1 = reader.ReadBytes(4);

            SkillId = reader.ReadInt32() & 0x3FFFFFF;

            //Not sure if it s a int32. or int16 or int64 or other thing 
            //When using a skill with many hit, each hit seem to have a different number (ex: 0, 1, 2, or 3)
            HitId = reader.ReadInt32();

            //No fucking idea. I think I see 3 different part in that thing
            Unknow2 = reader.ReadBytes(12);

            Amount = reader.ReadInt32();
            FlagsDebug = reader.ReadInt32();
            Flags = (SkillResultFlags) FlagsDebug;
            IsCritical = (reader.ReadByte() & 1) != 0;

            if (IsMana)
            {
                Amount = Math.Abs(Amount);
            }
        }

        //DEBUG
        public int FlagsDebug { get; }


        public int HitId { get; }

        //DEBUG
        public byte[] Unknow1 { get; }

        //DEBUG
        public byte[] Unknow2 { get; }


        public EntityId Source { get; private set; }
        public EntityId Target { get; private set; }
        public int Amount { get; }
        public int SkillId { get; private set; }
        public SkillResultFlags Flags { get; }
        public bool IsCritical { get; private set; }

        public bool IsMana => ((Flags & SkillResultFlags.Bit0) != 0) && ((Flags & SkillResultFlags.Heal) != 0);

        public bool IsHeal => ((Flags & SkillResultFlags.Bit0) == 0) && ((Flags & SkillResultFlags.Heal) != 0);
    }
}