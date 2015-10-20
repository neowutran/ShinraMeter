using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Tera.Protocol.Game.Parsing;

namespace Tera.Protocol.Game
{
    public class UnknownMessage : ParsedMessage
    {
        internal UnknownMessage(TeraMessageReader reader)
            : base(reader)
        {
        }
    }
}
