namespace Tera.Game.Messages
{
    public class SDespawnNpc : ParsedMessage
    {
        internal SDespawnNpc(TeraMessageReader reader) : base(reader)
        {
            Npc = reader.ReadEntityId();
            Position = reader.ReadVector3f();
            Dead = reader.ReadByte()== 5; // 1 = move out of view, 5 = death
        }

        public EntityId Npc { get; }
        public Vector3f Position { get; private set; }
        public bool Dead { get; }
    }
}