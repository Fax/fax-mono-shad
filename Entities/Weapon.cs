

using System.Collections.Specialized;

public class Weapon
{
    private readonly EventBus _bus;
    public float CooldownTimer;
    public Bullet CurrentBullet;
    private BulletRegistry _reg;

    public Weapon(EventBus bus)
    {
        _bus = bus;
        _bus.Subscribe<WeaponPickupEvent>(OnPickup);
        _reg = BulletRegistry.Instance;
        var b = _reg.RegisteredBullets.First(x => x.BulletType == 0);
        CurrentBullet = b;
    }
    public void Update(float dt) => CooldownTimer = MathF.Max(0, CooldownTimer - dt);
    void OnPickup(WeaponPickupEvent e)
    {
        var b = _reg.RegisteredBullets.First(x => x.BulletType == e.BulletType);
        CurrentBullet = b;
    }
    public void TryShoot(Vector2 origin, Vector2 direction)
    {
        if (CooldownTimer > 0) return;
        CooldownTimer = CurrentBullet.Cooldown;
        _bus.Publish(new ShootEvent(origin, direction.Normalized(), CurrentBullet.BulletSpeed, CurrentBullet.BulletType));
        Sfx.PlayShoot();
    }
}