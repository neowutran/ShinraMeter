// Copyright (c) Gothos
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Tera.Game.Messages;

namespace Tera.Game
{
    // NPCs and Mosters - Tera doesn't distinguish these
    public class NpcEntity : Entity
    {
        public NpcEntity(SpawnNpcServerMessage message)
            :base(message.Id)
        {
        }
    }
}
