using Microsoft.Xna.Framework;
public readonly struct ShootEvent : IGameEvent
{
    private readonly int id = IdManager.NextId();
    public readonly Vector2 Origin, Direction;
    public readonly float Speed;
    public readonly int BulletType;
    public ShootEvent(Vector2 origin, Vector2 direction, float speed, int bulletType)
    {
        Origin = origin;
        Direction = direction;
        Speed = speed;
        BulletType = bulletType;
    }

    int IGameEvent.EventID => id;
}
