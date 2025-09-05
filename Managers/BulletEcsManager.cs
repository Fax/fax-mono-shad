using Microsoft.Xna.Framework.Graphics;
using MonoGame.Extended.ECS;
using MonoGame.Extended.ECS.Systems;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

public class BulletEcsManager
{
    private readonly World _world;

    public BulletEcsManager(SpriteBatch spr, TextureManager textureManager)
    {

        _world = new WorldBuilder()
            .AddSystem(new LifeTickSystem())
            .AddSystem(new BulletSystem())
            .AddSystem(new BulletDrawSystem(spr, textureManager))
        .Build();

        _world.Initialize();
    }

    public void OnShoot(ShootEvent e)
    {
        var entt = _world.CreateEntity();
        entt.Attach(new BulletTransformComponent(e.Origin));
        var r = BulletRegistry.Instance.RegisteredBullets.First(x => x.BulletType == e.BulletType);
        entt.Attach(new BulletPhysicsComponent() { BulletSpeed = e.Speed, Velocity = e.Direction * r.BulletSpeed, Friction = r.Friction });
        entt.Attach(new BulletRenderComponent() { Color = r.Color, Size = r.Size });
        entt.Attach(new LifeComponent() { Life = r.Life });
    }

    public void Update(GameTime gameTime)
    {
        _world.Update(gameTime);
    }
    public void Draw(GameTime time)
    {
        _world.Draw(time);
    }
}

#region Systems

public class LifeTickSystem : EntityUpdateSystem
{
    private ComponentMapper<LifeComponent> _lifeMapper;
    private ComponentMapper<BulletTransformComponent> _transformMapper;
    private ComponentMapper<SplashDamageComponent> _splashDamageMapper;

    public LifeTickSystem() : base(Aspect.All(typeof(LifeComponent)))
    {
    }

    public override void Initialize(IComponentMapperService mapperService)
    {
        _lifeMapper = mapperService.GetMapper<LifeComponent>();
        _transformMapper = mapperService.GetMapper<BulletTransformComponent>();
        _splashDamageMapper = mapperService.GetMapper<SplashDamageComponent>();

    }



    public override void Update(GameTime gameTime)
    {
        var dt = (float)gameTime.ElapsedGameTime.TotalSeconds;
        foreach (var entityId in ActiveEntities)
        {
            var lifeComponent = _lifeMapper.Get(entityId);
            lifeComponent.Life -= dt;
            if (lifeComponent.Life < 0)
            {
                _lifeMapper.Delete(entityId);
                if (_transformMapper.TryGet(entityId, out BulletTransformComponent tr))
                {
                    if (_splashDamageMapper.TryGet(entityId, out SplashDamageComponent sd))
                    {


                    }
                }
                DestroyEntity(entityId);
            }
        }
    }
}

public class BulletSystem : EntityProcessingSystem
{
    private ComponentMapper<BulletPhysicsComponent> _physMapper;
    private ComponentMapper<BulletTransformComponent> _transformMapper;

    public BulletSystem() : base(Aspect.All(typeof(BulletTransformComponent), typeof(BulletPhysicsComponent)))
    {
    }

    public override void Initialize(IComponentMapperService mapperService)
    {
        _physMapper = mapperService.GetMapper<BulletPhysicsComponent>();
        _transformMapper = mapperService.GetMapper<BulletTransformComponent>();
    }

    public override void Process(GameTime gameTime, int entityId)
    {
        var dt = (float)gameTime.ElapsedGameTime.TotalSeconds;
        var transform = _transformMapper.Get(entityId);
        var phys = _physMapper.Get(entityId);

        phys.Velocity *= MathF.Pow(1 / phys.Friction, dt);
        transform.Position += dt * phys.Velocity * phys.BulletSpeed;
    }
}

public class BulletDrawSystem : EntityDrawSystem
{
    private ComponentMapper<BulletRenderComponent> _renderMapper;
    private ComponentMapper<BulletTransformComponent> _transformMapper;
    private SpriteBatch _spriteBatch;
    private TextureManager _textureManager;

    public BulletDrawSystem(SpriteBatch spriteBatch, TextureManager txmgr) : base(Aspect.All(typeof(BulletRenderComponent), typeof(BulletTransformComponent)))
    {
        _spriteBatch = spriteBatch;
        _textureManager = txmgr;
    }

    public override void Draw(GameTime gameTime)
    {
        var tex = _textureManager.GetTexture("bullet");
        Vector2 origin = new((float)(tex.Bounds.Width) / 2, (float)(tex.Bounds.Height) / 2);
        foreach (var entityId in ActiveEntities)
        {
            var transform = _transformMapper.Get(entityId);
            var sprite = _renderMapper.Get(entityId);

            _spriteBatch.Draw(
                tex,
                transform.Position.ToBoundingBox(sprite.Size),
                tex.Bounds,
                sprite.Color,
                  0,
                origin,
                SpriteEffects.None,
                0);
        }

    }

    public override void Initialize(IComponentMapperService mapperService)
    {
        _renderMapper = mapperService.GetMapper<BulletRenderComponent>();
        _transformMapper = mapperService.GetMapper<BulletTransformComponent>();
    }
}

#endregion
#region Components 

public class BulletComponent
{
    public int BulletType = 0;

    public Vector2 Position, Velocity;
    public float Cooldown = 1.0f;
    public int EntityId = IdManager.NextId();
    public float Damage = 5f;
}

public class BulletShootingComponent
{
    public float Cooldown = 1.0f;
    public float Damage = 5f;
}
public class SplashDamageComponent
{
    public float SplashDamageRange = 0.0f;
    public float SplashDamage = 0.0f;
}
public class BulletPhysicsComponent
{
    public float BulletSpeed = 1.0f;
    public Vector2 Velocity; // encodes direction and velocity
    public float Friction = 2f;
}
public class LifeComponent
{
    public float Life = 2.0f;
    public float Armor = 0.0f;
}
public class BulletRenderComponent
{
    public Color Color = new(new Vector4(1.0f));
    public Vector2 Size = new(10, 10);
}
public class BulletTransformComponent
{
    public Vector2 Position;
    public BulletTransformComponent(Vector2 position)
    {
        Position = position;
    }
}
#endregion