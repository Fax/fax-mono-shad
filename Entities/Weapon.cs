

public class Weapon
{
    private readonly EventBus _bus;
    public float Cooldown = 0.12f;
    float _t;

    public int BulletType = 1;

    public Weapon(EventBus bus)
    {
        _bus = bus;
        _bus.Subscribe<WeaponPickupEvent>(OnPickup);
    }
    public void Update(float dt) => _t = MathF.Max(0, _t - dt);
    void OnPickup(WeaponPickupEvent e)
       => BulletType = e.BulletType;
    public void TryShoot(Vector2 origin, Vector2 direction, float speed)
    {
        if (_t > 0) return;
        _t = Cooldown;
        _bus.Publish(new ShootEvent(origin, direction.Normalized(), speed, BulletType));
        Sfx.PlayShoot();
    }
}