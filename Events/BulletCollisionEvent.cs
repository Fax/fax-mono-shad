public readonly struct BulletCollisionEvent : IGameEvent
{
    private readonly int id = IdManager.NextId();
    int IGameEvent.EventID => id;
    public readonly int BulletId;
    public readonly int CollisionEntityId;
    public BulletCollisionEvent(int entityId, int collisionEntityId)
    {
        BulletId = entityId;
        CollisionEntityId = collisionEntityId;
    }
}
