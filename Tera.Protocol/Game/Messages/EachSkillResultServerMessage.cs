using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Tera.Protocol.Game.Parsing;

namespace Tera.Protocol.Game
{
    public class EachSkillResultServerMessage : ParsedMessage
    {
        public EntityId Source { get; private set; }
        public EntityId Target { get; private set; }
        public int Amount { get; private set; }
        public int SkillId { get; private set; }
        public SkillResultFlags SkillResultFlags { get; private set; }
        public bool IsCritical { get; private set; }

        public bool IsHeal { get { return (SkillResultFlags & SkillResultFlags.Heal) != 0; } }
        public int Damage { get { return IsHeal ? 0 : Amount; } }
        public int Heal { get { return IsHeal ? Amount : 0; } }

        internal EachSkillResultServerMessage(TeraMessageReader reader)
            : base(reader)
        {
            reader.Skip(4);
            Source = reader.ReadEntityId();
            Target = reader.ReadEntityId();
            reader.Skip(4);
            SkillId = reader.ReadInt32() & 0x3FFFFFF;
            reader.Skip(16);
            Amount = reader.ReadInt32();
            SkillResultFlags = (SkillResultFlags)reader.ReadInt32();
            IsCritical = (reader.ReadByte() & 1) != 0;
        }
    }
}
