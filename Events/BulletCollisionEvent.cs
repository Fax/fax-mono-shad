public readonly struct BulletCollisionEvent : IGameEvent
{
    private readonly int id = IdManager.NextId();
    int IGameEvent.EventID => id;
    public readonly int BulletId;
    public readonly int CollisionEntityId;
    public readonly float BulletDamage;
    public BulletCollisionEvent(int entityId, int collisionEntityId, float bulletDamage = 1.0f)
    {
        BulletId = entityId;
        CollisionEntityId = collisionEntityId;
        BulletDamage = bulletDamage;
    }
}


public readonly struct AreaCollisionEvent : IGameEvent
{
    private readonly int id = IdManager.NextId();
    int IGameEvent.EventID => id;
    public readonly int AreaId;
    public readonly int CollisionEntityId;
    public readonly float AreaDamage;
    public AreaCollisionEvent(int entityId, int collisionEntityId, float dmg )
    {
        AreaId = entityId;
        CollisionEntityId = collisionEntityId;
        AreaDamage = dmg;
    }
}