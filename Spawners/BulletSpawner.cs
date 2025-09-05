



using Microsoft.Xna.Framework.Graphics;
using MonoGame.Extended.ECS;
using MonoGame.Extended.ECS.Systems;

public class BulletSpawner
{
    private readonly Random rng = new();

    public Bullet CreateBullet(ShootEvent e)
    {
        var toSpawn = BulletRegistry.Instance.RegisteredBullets.First(x => x.BulletType == e.BulletType);


        float angle = (float)((rng.NextDouble() * 2 - 1) * toSpawn.Spread);

        // Rotate the direction by this angle
        Vector2 dir = e.Direction;
        float cos = MathF.Cos(angle);
        float sin = MathF.Sin(angle);

        Vector2 spreadDir = new(
            dir.X * cos - dir.Y * sin,
            dir.X * sin + dir.Y * cos
        );
        spreadDir = Vector2.Normalize(spreadDir);

        var newInstance = toSpawn with { Position = e.Origin, Velocity = spreadDir * e.Speed };
        return newInstance;

        //switch (e.BulletType)
        //{
        //    case 2:
        //        return new MagicalBullet
        //        {
        //            Position = e.Origin,
        //            Velocity = e.Direction * e.Speed,
        //        };
        //    case 1:
        //        return new ExplosiveBullet
        //        {
        //            Position = e.Origin,
        //            Velocity = e.Direction * e.Speed,
        //            Color = new Color(.8f, .3f, .2f, .9f)
        //        };

        //    case 0:
        //    default:
        //        return new Bullet
        //        {
        //            Position = e.Origin,
        //            Velocity = e.Direction * e.Speed,
        //            BulletType = e.BulletType
        //        };
        //}
    }
    private readonly List<Bullet> _bullets;
    private readonly EventBus _bus;

    public BulletSpawner(EventBus bus, List<Bullet> bullets)
    {
        _bullets = bullets;
        _bus = bus;
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
    {
        _bullets.Add(CreateBullet(e));
    }

    public void Update(float dt)
    {

        _bullets.ForEach(b =>
        {
            b.Life -= dt;
            b.Velocity *= MathF.Pow(1 / b.Friction, dt);
            b.Position += dt * b.Velocity * b.BulletSpeed;
            if (b.Life < 0 && b.SplashDamage > 0.0f)
            {
                _bus.Publish(new SplashDamageEvent(b.Position, b.SplashDamage, b.SplashDamageRange));
            }
        }); // decrease life

        // should trigger big booms here.
        _bullets.RemoveAll(b => b.Life <= 0);

    }


}