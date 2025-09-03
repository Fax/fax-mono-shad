public readonly struct WeaponPickupEvent : IGameEvent
{

    private readonly int id = IdManager.NextId();
    int IGameEvent.EventID => id;
    public readonly int BulletType = 0;
    public readonly int EntityId;
    public readonly int PlayerId;
    public WeaponPickupEvent(int btype, int entityId, int playerId)
    {
        EntityId = entityId;
        PlayerId = playerId;
        BulletType = btype;
    }
}