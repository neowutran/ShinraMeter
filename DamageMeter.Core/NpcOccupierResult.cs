using Tera.Game;
using Tera.Game.Messages;

namespace DamageMeter
{
    public class NpcOccupierResult
    {
        public NpcOccupierResult(SNpcOccupierInfo message)
        {
            HasReset = message.Target == EntityId.Empty;
            Npc = message.NPC;
        }

        public bool HasReset { get; private set; }

        public EntityId Npc { get; private set; }
    }
}