namespace Tera.Game
{
    public class ProjectileEntity : Entity
    {
        public ProjectileEntity(EntityId id, EntityId ownerId, Entity owner)
            : base(id)
        {
            OwnerId = ownerId;
            Owner = owner;
        }

        public EntityId OwnerId { get; private set; }
        public Entity Owner { get; private set; }
    }
}