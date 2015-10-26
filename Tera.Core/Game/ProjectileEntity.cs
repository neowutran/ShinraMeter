namespace Tera.Game
{
    public class ProjectileEntity : Entity
    {
        public EntityId OwnerId { get; private set; }
        public Entity Owner { get; private set; }

        public ProjectileEntity(EntityId id, EntityId ownerId, Entity owner)
            : base(id)
        {
            OwnerId = ownerId;
            Owner = owner;
        }
    }
}
