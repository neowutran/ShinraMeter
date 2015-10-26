namespace Tera.Game
{
    public class Projectile : Entity
    {
        public EntityId OwnerId { get; private set; }
        public Entity Owner { get; private set; }

        public Projectile(EntityId id, EntityId ownerId, Entity owner)
            : base(id)
        {
            OwnerId = ownerId;
            Owner = owner;
        }
    }
}
