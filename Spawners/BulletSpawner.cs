



public class BulletSpawner : ISpawner<Bullet>
{

    public Bullet CreateBullet(ShootEvent e)
    {
        switch (e.BulletType)
        {
            case 2:
                return new MagicalBullet
                {
                    Position = e.Origin,
                    Velocity = e.Direction * e.Speed,
                };
            case 1:
                return new ExplosiveBullet
                {
                    Position = e.Origin,
                    Velocity = e.Direction * e.Speed,
                    Color = new Color(.8f,.3f,.2f,.9f)
                };

            case 0:
            default:
                return new Bullet
                {
                    Position = e.Origin,
                    Velocity = e.Direction * e.Speed,
                    BulletType = e.BulletType
                };
        }
    }
    private readonly List<Bullet> _bullets;
    public BulletSpawner(EventBus bus, List<Bullet> bullets)
    {
        _bullets = bullets;
        bus.Subscribe<ShootEvent>(OnShoot);
        bus.Subscribe<BulletCollisionEvent>(OnCollide);
    }
    void OnCollide(BulletCollisionEvent evt)
    {
        var b = _bullets.FirstOrDefault(x => x.EntityId == evt.BulletId);
        if (b == null) return;
        b.Life = 0f;
    }
    void OnShoot(ShootEvent e)
        => _bullets.Add(CreateBullet(e));

    public void Update(float dt)
    {
        _bullets.ForEach(b =>
        {
            b.Life -= dt;
            b.Position += dt * b.Velocity;
        }); // decrease life

        // should trigger big booms here.
        _bullets.RemoveAll(b => b.Life <= 0);

    }
}