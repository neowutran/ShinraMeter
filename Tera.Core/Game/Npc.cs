using Tera.Game.Messages;

namespace Tera.Game
{
    // NPCs and Mosters - Tera doesn't distinguish these
    public class Npc : Entity
    {
        public Npc(SpawnNpcServerMessage message)
            :base(message.Id)
        {
        }
    }
}
