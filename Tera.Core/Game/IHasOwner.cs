using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Tera.Game
{
    interface IHasOwner
    {
        EntityId OwnerId { get; }
        Entity Owner { get; }
    }
}
