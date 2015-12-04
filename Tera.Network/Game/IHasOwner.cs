namespace Tera.Game
{
    internal interface IHasOwner
    {
        EntityId OwnerId { get; }
        Entity Owner { get; }
    }
}