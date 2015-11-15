namespace Tera.Game
{
    // An object with an Id that can be spawned or deswpawned in the game world
    public class Entity
    {
        public Entity(EntityId id)
        {
            Id = id;
        }

        public EntityId Id { get; }

        public override string ToString()
        {
            return $"{GetType().Name} {Id}";
        }
    }
}