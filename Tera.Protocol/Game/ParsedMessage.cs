using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Tera.Protocol.Game.Parsing;

namespace Tera.Protocol.Game
{
    public class ParsedMessage : Message
    {
        public string OpCodeName { get; private set; }

        internal ParsedMessage(TeraMessageReader reader)
            : base(reader.Message.Time, reader.Message.Direction, reader.Message.Data)
        {
            OpCodeName = reader.OpCodeName;
        }
    }
}
