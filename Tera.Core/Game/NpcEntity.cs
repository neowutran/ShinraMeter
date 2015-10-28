using Tera.Game.Messages;

namespace Tera.Game
{
    // NPCs and Mosters - Tera doesn't distinguish these
    public class NpcEntity : Entity
    {
        public NpcEntity(SpawnNpcServerMessage message)
            : base(message.Id)
        {
        }
    }
}