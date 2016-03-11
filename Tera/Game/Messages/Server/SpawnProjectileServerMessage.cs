namespace Tera.Game.Messages
{
    public class SpawnProjectileServerMessage : ParsedMessage
    {
        internal SpawnProjectileServerMessage(TeraMessageReader reader)
            : base(reader)
        {
            Id = reader.ReadEntityId();
            reader.Skip(4);
            Model = reader.ReadInt32();
            Start = reader.ReadVector3f();
            Finish = reader.ReadVector3f();
            reader.Skip(5);
            OwnerId = reader.ReadEntityId();
        }

        public EntityId Id { get; private set; }
        public int Model { get; private set; }
        public Vector3f Start { get; private set; }
        public Vector3f Finish { get; private set; }
        public EntityId OwnerId { get; private set; }
    }
}