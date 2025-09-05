public class EnemyManager
{
    private Random rnd = new();
    private readonly EventBus _bus;
    public List<EnemyEntity> Enemies;
    public int MaxEntities = 15;
    public float MinRange = 400.0f;
    public float MaxRange = 600.0f;
    public Vector2 SpawnOrigin = new(400, 400);
    public EnemyManager(EventBus bus, List<EnemyEntity> entities)
    {
        _bus = bus;
        Enemies = entities;
        _bus.Subscribe<BulletCollisionEvent>(BulletHit);
        _bus.Subscribe<AreaCollisionEvent>(AreaDamageCollision);
    }
    public void AreaDamageCollision(AreaCollisionEvent e)
    {
        var enemyCollided = Enemies.FirstOrDefault(x => x.EntityId == e.CollisionEntityId);
        if (enemyCollided == null) return;
        enemyCollided.Life -= e.AreaDamage;
        if (enemyCollided.Life > 0) return;
        Kill(enemyCollided);
    }
    public void BulletHit(BulletCollisionEvent evt)
    {
        var enemyCollided = Enemies.FirstOrDefault(x => x.EntityId == evt.CollisionEntityId);
        if (enemyCollided == null) return;
        enemyCollided.Life -= evt.BulletDamage;
        if (enemyCollided.Life > 0) return;
        Kill(enemyCollided);
    }

    private void Kill(EnemyEntity e)
    {
        e.Active = false;
        _bus.Publish<SpawnExperienceEvent>(new SpawnExperienceEvent(100, e.Position));
        Sfx.PlayBoom();
    }

    public float Cooldown = .15f;
    public void Spawn()
    {
        if (Enemies.Count >= MaxEntities) return;
        // find a random position, possibly outside the area
        var anglef = rnd.NextSingle();
        // match angle 0.0-1.0 to the angle relative to the centre of the screen.
        var r = (float)Math.Tau * anglef;
        float d = MinRange + rnd.NextSingle() * (MaxRange - MinRange);
        Vector2 direction = new(MathF.Sin(r), -MathF.Cos(r));
        var position = SpawnOrigin + direction * d;
        Enemies.Add(new EnemyEntity() { Position = position });
    }

    float _t;
    public void Update(float dt, Vector2 playerPosition)
    {
        _t = MathF.Max(0, _t - dt);
        if (_t <= 0)
        {
            _t = Cooldown;
            Spawn();
        }
        foreach (var entity in Enemies.Where(x => x.Active))
        {
            entity.Position += dt * entity.Speed * (playerPosition - entity.Position).Normalized();
        }

        Enemies.RemoveAll(x => !x.Active);
    }

}